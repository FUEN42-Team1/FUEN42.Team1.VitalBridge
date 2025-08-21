using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Team1.VitalBridge.BackStage.Models.Utilities;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.EFModels;
using Team1.VitalBridge.Frontend.Models.DTOs;

namespace Team1.VitalBridge.Frontend.Models.Services
{

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtService _jwt;
        private readonly IConfiguration _cfg;
        private readonly IHttpContextAccessor _http;

        public AuthService(AppDbContext db, IJwtService jwt, IConfiguration cfg, IHttpContextAccessor http)
        {
            _db = db; _jwt = jwt; _cfg = cfg; _http = http;
        }

        // ==== Public APIs ====

        public async Task RegisterAsync(RegisterDto dto)
        {

            if (await _db.Users.AnyAsync(x => x.Email == dto.Email))
                throw new InvalidOperationException("Email 已被註冊");


            var roleId = await _db.Roles
                .Where(r => r.RoleCode == "Member")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (roleId == 0) // 如果是 int 主鍵
                throw new InvalidOperationException("系統沒有預設角色 Member，請先建立角色資料");

            // Email驗證碼
            var ConfirmCodeToken = Guid.NewGuid().ToString("N");




            //建立user物件
            var user = new User
            {
                UserId = Guid.NewGuid().ToString("N"),
                Email = dto.Email,
                Name = dto.Name,
                Password = HashUtility.HashPassword(dto.Password),
                AccountType = "Member",
                Status = "unverified",
                ConfirmCode = ConfirmCodeToken, // 產生確認碼
                ConfirmCodeExpiresAt = DateTime.UtcNow.AddHours(1), // 確認碼有效期為1小時
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };


            //建立MemberProfile物件
            var MemberProfile = new MemberProfile
            {
                User = user,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };




            //建立UserRoles物件（假設預設角色為 "Member"）
            var userRole = new UserRole
            {
                User = user,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow

            };

            _db.UserRoles.Add(userRole);


            _db.AddRange(user, MemberProfile, userRole);
            await _db.SaveChangesAsync();



            // 發送驗證郵件
            string verifyLink = $"https://localhost:5500/verify?email={dto.Email}&token={ConfirmCodeToken}";
            //輸出到debug控制台
            Console.WriteLine($"發送驗證郵件到 {dto.Email}，驗證連結：{verifyLink}");


            //            string sql = $@"
            //    EXEC msdb.dbo.sp_send_dbmail
            //    @profile_name = 'VitalBridge',
            //    @recipients = '{dto.Email}', 
            //    @subject = '【VitalBridge】帳號驗證信',
            //    @body = '
            //親愛的 {dto.Name} 您好：

            //感謝您註冊 VitalBridge 平台。
            //請點擊以下連結完成帳號驗證：

            //{verifyLink}

            //如果您沒有註冊過 VitalBridge，請忽略此封信件。

            //-- VitalBridge 系統通知
            //',
            //    @body_format = 'TEXT';";

            //_db.Database.ExecuteSqlRaw(sql);


        }

        public async Task<TokenRes> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);
            if (user == null) throw new UnauthorizedAccessException("帳號或密碼錯誤");
            if (!HashUtility.VerifyPassword(dto.Password, user.Password))
                throw new UnauthorizedAccessException("帳號或密碼錯誤");
            if (user.Status == "banned")
                throw new UnauthorizedAccessException("帳號已停權");
            if (DateTime.UtcNow < user.LockedUntil)
            {
                var remaining = user.LockedUntil - DateTime.UtcNow;

                string msg;
                if (remaining.Value.TotalMinutes < 60)
                {
                    // 只顯示分鐘
                    var minutes = (int)Math.Ceiling(remaining.Value.TotalMinutes);
                    msg = $"帳號已鎖定，請 {minutes} 分鐘後再試";
                }
                else
                {
                    // 顯示 小時 + 分鐘
                    int hours = (int)remaining.Value.TotalHours;
                    int minutes = remaining.Value.Minutes;
                    msg = $"帳號已鎖定，請 {hours} 小時 {minutes} 分鐘後再試";
                }

                throw new UnauthorizedAccessException(msg);
            }
            if (user.Status == "unverified")
                throw new UnauthorizedAccessException("帳號尚未驗證，請先驗證後再登入");
            if (user.Status == "frozen")
                throw new UnauthorizedAccessException("帳號已凍結，請聯繫管理員");




            // TODO: 你的角色取得邏輯
            var roles = await GetUserRolesAsync(user.Id);

            var access = _jwt.CreateAccessToken(user, roles);
            var refresh = CreateRefreshJwt(user); // 無表：簽一顆 Refresh-JWT

            SetRefreshCookie(refresh, DateTime.UtcNow.AddDays(_jwt.RefreshDays));
            IssueXsrfCookie(_jwt.RefreshDays);

            return new TokenRes(access, _jwt.AccessMinutes * 60);
        }

        public async Task<TokenRes> RefreshAsync()
        {
            var ctx = _http.HttpContext!;
            if (!ValidateXsrf(ctx.Request)) throw new UnauthorizedAccessException("CSRF 驗證失敗");

            var refreshJwt = ctx.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshJwt)) throw new UnauthorizedAccessException("缺少 Refresh Token");

            var principal = ValidateRefreshJwt(refreshJwt);
            if (principal == null) throw new UnauthorizedAccessException("Refresh Token 無效");

            var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? principal.FindFirstValue("sub");
            if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException("Token 主體錯誤");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) throw new UnauthorizedAccessException("使用者不存在");

            // sst 檢查（密碼變更/狀態變更時擋下舊 refresh）
            var sstClaim = principal.FindFirst("sst")?.Value;
            var sstNow = BuildSecurityStamp(user);
            if (!string.Equals(sstClaim, sstNow, StringComparison.Ordinal))
                throw new UnauthorizedAccessException("使用者狀態已變更，請重新登入");

            var roles = await GetUserRolesAsync(user.Id);
            var newAccess = _jwt.CreateAccessToken(user, roles);
            var newRefresh = CreateRefreshJwt(user); // 旋轉

            SetRefreshCookie(newRefresh, DateTime.UtcNow.AddDays(_jwt.RefreshDays));
            IssueXsrfCookie(_jwt.RefreshDays);

            return new TokenRes(newAccess, _jwt.AccessMinutes * 60);
        }

        public Task LogoutAsync()
        {
            // 無表：只能刪瀏覽器端 Cookie
            var res = _http.HttpContext!.Response;
            res.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
            res.Cookies.Delete("XSRF-TOKEN", new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/"
            });
            return Task.CompletedTask;
        }

        public async Task<object?> GetUserInfoAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            // 假設有 UserRoles 中介表，Role 有 Name 欄位
            var roles = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            return new
            {
                user.Id,
                user.Email,
                user.Name,
                user.AccountType,
                Roles = roles // 回傳角色名稱陣列
            };
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return; // 不洩漏帳號存在與否

            // 產生 token
            var token = Guid.NewGuid().ToString("N");
            user.ResetPasswordConfirmCode = token;
            user.ResetPasswordConfirmCodeExpiresAt = DateTime.UtcNow.AddHours(1);

            await _db.SaveChangesAsync();

            // 建立重設密碼連結
            var resetLink = $"https://localhost:5500/reset-password?email={email}&token={token}";
            Console.WriteLine($"發送重設密碼郵件到 {email}，連結：{resetLink}");

            // TODO: 實際寄信
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x =>
                x.ResetPasswordConfirmCode == token &&
                x.ResetPasswordConfirmCodeExpiresAt > DateTime.UtcNow);

            if (user == null) return false;

            user.Password = HashUtility.HashPassword(newPassword);
            user.ResetPasswordConfirmCode = null;
            user.ResetPasswordConfirmCodeExpiresAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        // ==== Helpers（服務內部） ====

        private async Task<string[]> GetUserRolesAsync(int userId)
        {
            // TODO: 依你的資料表實作；暫時給空陣列
            await Task.CompletedTask;
            return Array.Empty<string>();
        }

        // 無表：直接用 JwtService 的 Refresh 簽章（可直接呼叫你已實作的 CreateRefreshJwt/ValidateRefreshJwt）
        private string CreateRefreshJwt(User user)
        {
            // 若你的 JwtService 已有 CreateRefreshJwt(user)，直接呼叫即可
            // 這裡示範內建：加入 typ=rt 與 sst
            var refreshKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_cfg["Jwt:RefreshKey"]!)
            );
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                refreshKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256
            );

            var claims = new[]
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("typ", "rt"),
                new Claim("sst", BuildSecurityStamp(user)),
                new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(_jwt.RefreshDays),
                signingCredentials: creds
            );
            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal? ValidateRefreshJwt(string refreshJwt)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var param = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = _cfg["Jwt:Issuer"],
                ValidAudience = _cfg["Jwt:Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_cfg["Jwt:RefreshKey"]!)
                ),
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            try
            {
                var principal = handler.ValidateToken(refreshJwt, param, out var _);
                if (principal.FindFirst("typ")?.Value != "rt") return null;
                return principal;
            }
            catch { return null; }
        }

        // 不新增欄位：用 Password 導出安全戳
        private string BuildSecurityStamp(User user)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(user.Password ?? ""));
            return Convert.ToHexString(bytes);
        }

        private void SetRefreshCookie(string token, DateTime expiresUtc)
        {
            _http.HttpContext!.Response.Cookies.Append("refresh_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = expiresUtc,
                Path = "/"
            });
        }

        private void IssueXsrfCookie(int days)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            _http.HttpContext!.Response.Cookies.Append("XSRF-TOKEN", token, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(days),
                Path = "/"
            });
        }

        private bool ValidateXsrf(HttpRequest req)
        {
            if (!req.Cookies.TryGetValue("XSRF-TOKEN", out var cookie)) return false;
            if (!req.Headers.TryGetValue("X-XSRF-TOKEN", out var header)) return false;
            return string.Equals(cookie, header, StringComparison.Ordinal);
        }

    }
}


