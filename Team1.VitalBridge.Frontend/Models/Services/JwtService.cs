using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Team1.VitalBridge.BackStage.Models.Utilities;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Models.Services
{

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _config;

        public int AccessMinutes { get; }    // Access Token 有效時間
        public int RefreshDays { get; }      // Refresh Token 有效時間

        public JwtService(IConfiguration config)
        {
            _config = config;
            AccessMinutes = int.Parse(_config["Jwt:AccessMinutes"] ?? "20");
            RefreshDays = int.Parse(_config["Jwt:RefreshDays"] ?? "7");
        }

        // ===== Access Token =====
        public string CreateAccessToken(User user, string[] roles)
        {
            var accessKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:AccessKey"]!));
            var creds = new SigningCredentials(accessKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 新增這行
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("typ", "at") // 明確標記 access token
            };
            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(AccessMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ===== Refresh Token：用 JWT 表示，存在 Cookie =====
        // sst = security stamp，利用既有資料導出（無需改 DB），密碼一變舊 token 立即失效
        private string BuildSecurityStamp(User user)
        {
            // 把 PasswordHash 再經過一次 bcrypt（不會太慢，因為只在 refresh 時用）
            return HashUtility.HashPassword(user.Password ?? "");
        }

        public string CreateRefreshJwt(User user)
        {
            var refreshKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:RefreshKey"]!));
            var creds = new SigningCredentials(refreshKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // 新增這行
        new Claim("typ", "rt"),
        new Claim("sst", BuildSecurityStamp(user)), // << 用 HashUtility 產出安全戳
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddDays(RefreshDays),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public ClaimsPrincipal? ValidateRefreshJwt(string refreshJwt, out SecurityToken? validatedToken)
        {
            validatedToken = null;
            var handler = new JwtSecurityTokenHandler();
            var param = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:RefreshKey"]!)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            try
            {
                var principal = handler.ValidateToken(refreshJwt, param, out validatedToken);
                // 額外檢查 typ=rt
                if (principal.FindFirst("typ")?.Value != "rt") return null;
                return principal;
            }
            catch
            {
                return null;
            }
        }

        // 你原本的：提供給「純隨機字串」情境；此方案用不到，可保留
        public string GenerateRefreshToken(int bytes = 32)
        {
            var randomNumber = new byte[bytes];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }


    }

}
