using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.Utilities;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Team1.VitalBridge.BackStage.Models.ViewModels.Institutions;

namespace Team1.VitalBridge.BackStage.Controllers.Institutions
{
    [Authorize(AuthenticationSchemes = "InstitutionJwtScheme")]
    [Route("Institution/[controller]/[action]")]
    public class InstitutionAuthController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;
        private readonly LocationService _locationService;

        public InstitutionAuthController(AppDbContext context, IConfiguration configuration, JwtService jwtService , LocationService locationService)
        {
            this._context = context;
            this._configuration = configuration;
            this._jwtService = jwtService;
            this._locationService = locationService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        //[AllowAnonymous]
        //[HttpPost]
        //public async Task<IActionResult> Login(InstitutionLoginViewModel vm)
        //{

        //    if (!ModelState.IsValid)
        //    {
        //        return View(vm);
        //    }

        //    //先檢查機構狀態
        //    var inst = await _context.Institutions
        //           .AsNoTracking()
        //           .SingleOrDefaultAsync(x => x.InstitutionCode == vm.InstitutionCode);

        //    if (inst == null)
        //    {
        //        ModelState.AddModelError(string.Empty, "機構代碼、帳號或密碼不正確");
        //        return View(vm);
        //    }

        //    if (inst.IsBanned) {
        //        ModelState.AddModelError(string.Empty, "機構已停權，請聯絡管理員");
        //        return View(vm);
        //    }

        //    if (inst.Status == "Pending") {
        //        ModelState.AddModelError(string.Empty, "機構審核中");
        //        return View(vm);
        //    }


        //    //接著處理帳號






        //    // 檢查使用者是否存在

        //    var InstitutionUser = await _context.Users
        //       .AsNoTracking()
        //       .AsSplitQuery()
        //       .Include(au => au.UserRoles)
        //           .ThenInclude(aur => aur.Role) 
        //               .ThenInclude(ar => ar.RolePermissions) 
        //                   .ThenInclude(arp => arp.Permission)
        //         .Include(au => au.InstitutionProfile) 
        //         .ThenInclude(ip => ip.Institution) 
        //       .SingleOrDefaultAsync(au => au.Email == vm.Email);


        //    // 驗證使用者是否存在且密碼正確
        //    if (InstitutionUser == null || !HashUtility.VerifyPassword(vm.Password, InstitutionUser.Password))
        //    {
        //        ModelState.AddModelError("", "無效的電子郵件或密碼。");
        //        return View(vm);
        //    }

        //    //檢查是否已驗證開通
        //    if (InstitutionUser.Status == "unverified")
        //    {
        //        ModelState.AddModelError("", "您的帳號尚未啟用，請檢查電子郵件進行驗證。");
        //        return View(vm);
        //    }

        //    //if (!adminUser.AdminUsersRoles.Any(aur => aur.Role != null &&
        //    //                          (aur.Role.RoleCode.Trim().Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) ||
        //    //                           aur.Role.RoleCode.Trim().Equals("DATA_BROWSER", StringComparison.OrdinalIgnoreCase))))
        //    //{
        //    //    ModelState.AddModelError("", "此帳號無權登入管理後台。");
        //    //    return View(vm);
        //    //}



        //    // 建立 Claims 列表 
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, InstitutionUser.UserId), // 使用者唯一ID
        //    new Claim(ClaimTypes.Email, InstitutionUser.Email),
        //    new Claim(ClaimTypes.Name, InstitutionUser.Name)
        //};


        //    if (InstitutionUser.UserRoles?.Any() == true)
        //    {
        //        // 使用 SelectMany 取得所有唯一的角色代碼
        //        var roleCodes = InstitutionUser.UserRoles
        //            .Where(r => r.Role != null && !string.IsNullOrEmpty(r.Role.RoleCode))
        //            .Select(r => r.Role.RoleCode)
        //            .Distinct()
        //            .ToList();

        //        // 將所有角色代碼轉換為 Claim 物件並添加到 claims 列表中
        //        claims.AddRange(roleCodes.Select(roleCode => new Claim(ClaimTypes.Role, roleCode)));

        //        // 使用 SelectMany 取得所有唯一的權限代碼
        //        var permissionCodes = InstitutionUser.UserRoles
        //            .Where(r => r.Role?.RolePermissions?.Any() == true)
        //            .SelectMany(r => r.Role.RolePermissions)
        //            .Where(p => p.Permission != null && !string.IsNullOrEmpty(p.Permission.PermissionCode))
        //            .Select(p => p.Permission.PermissionCode)
        //            .Distinct()
        //            .ToList();

        //        // 將所有權限代碼轉換為 Claim 物件並添加到 claims 列表中
        //        claims.AddRange(permissionCodes.Select(permissionCode => new Claim("Permission", permissionCode)));
        //    }


        //    // 生成 JWT Token
        //    // 確保 Expires 時間一致
        //    var expiresMinutes = double.Parse(_configuration["JwtSettings:ExpirationMinutes"]);
        //    var jwtToken = _jwtService.GenerateToken(claims, expiresMinutes, "InstitutionJwtScheme");// 生成 JWT Token
        //    _jwtService.SetTokenCookie(jwtToken, expiresMinutes, Response, "institution_auth_token", "/Institution");// 將 JWT 存入 HttpOnly Cookie

        //    return RedirectToAction("Index", "InstitutionHome");

        //}


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(InstitutionLoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // 1) 先找機構（把 ban / pending 擋掉）
            var inst = await _context.Institutions
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.InstitutionCode == vm.InstitutionCode);

            if (inst == null)
            {
                ModelState.AddModelError(string.Empty, "機構代碼、帳號或密碼不正確");
                return View(vm);
            }
            if (inst.IsBanned)
            {
                ModelState.AddModelError(string.Empty, "機構已停權，請聯絡管理員");
                return View(vm);
            }
            if (inst.Status == "Pending")
            {
                ModelState.AddModelError(string.Empty, "機構審核中");
                return View(vm);
            }

            // 2) 以【Email + 機構代碼】雙條件抓人：確保此帳號屬於該機構
            var user = await _context.Users
                .AsNoTracking()
                .AsSplitQuery()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.InstitutionProfile)
                    .ThenInclude(ip => ip.Institution)
                .SingleOrDefaultAsync(u =>
                    u.Email == vm.Email &&
                    u.InstitutionProfile != null &&
                    u.InstitutionProfile.Institution.InstitutionCode == vm.InstitutionCode);

            // 3) 帳密驗證（注意：若上一步找不到人，同樣回覆一般錯誤）
            if (user == null || !HashUtility.VerifyPassword(vm.Password, user.Password))
            {
                ModelState.AddModelError("", "無效的電子郵件或密碼。");
                return View(vm);
            }

            // 4) 帳號狀態（未驗證不給登）
            if (user.Status == "unverified")
            {
                ModelState.AddModelError("", "您的帳號尚未啟用，請檢查電子郵件進行驗證。");
                return View(vm);
            }

            // 5) 限制只能登入「機構後台」的角色（若你有 RoleType 區分，建議強化）
            //    例如只允許 RoleType == "Institution" 的角色能進入本後台
            bool hasInstitutionRole = user.UserRoles?.Any(ur => ur.Role != null && ur.Role.RoleType == "Institution" && ur.Role.IsActive) == true;
            if (!hasInstitutionRole)
            {
                ModelState.AddModelError("", "此帳號無權登入機構後台。");
                return View(vm);
            }

            // 6) 建立 Claims（加入 Institution 的識別，之後授權/多租戶查詢會更方便）
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId),
        new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
        new Claim(ClaimTypes.Name, user.Name ?? string.Empty),

        // 多租戶關鍵：把機構 Id/Code 也寫進去
        new Claim("InstitutionId", user.InstitutionProfile!.InstitutionId.ToString()),
        new Claim("InstitutionCode", user.InstitutionProfile!.Institution!.InstitutionCode),
    };

            if (user.UserRoles?.Any() == true)
            {
                var roleCodes = user.UserRoles
                    .Where(r => r.Role != null && !string.IsNullOrWhiteSpace(r.Role.RoleCode))
                    .Select(r => r.Role!.RoleCode)
                    .Distinct();

                claims.AddRange(roleCodes.Select(code => new Claim(ClaimTypes.Role, code)));

                var permissionCodes = user.UserRoles
                    .Where(r => r.Role?.RolePermissions?.Any() == true)
                    .SelectMany(r => r.Role!.RolePermissions!)
                    .Where(p => p.Permission != null && !string.IsNullOrWhiteSpace(p.Permission!.PermissionCode))
                    .Select(p => p.Permission!.PermissionCode)
                    .Distinct();

                claims.AddRange(permissionCodes.Select(p => new Claim("Permission", p)));
            }

            // 7) 發 Token（注意 audienceKey 與 Cookie 名稱區分不同後台）
            var expiresMinutes = double.Parse(_configuration["JwtSettings:ExpirationMinutes"]);
            var jwtToken = _jwtService.GenerateToken(claims, expiresMinutes, "InstitutionJwtScheme");
            _jwtService.SetTokenCookie(jwtToken, expiresMinutes, Response, "institution_auth_token", "/Institution");

            return RedirectToAction("Index", "InstitutionHome");
        }








        //申請機構帳號
        //先顯示註冊頁面
        //填入資料後驗證OK便先建立機構資料(狀態設為審核中)
        //
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Register() {

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(InstitutionRegisterViewModel vm) {


            // 檢查機構代碼是否已存在
            if (await _context.Institutions.AsNoTracking()
                .AnyAsync(i => i.InstitutionCode == vm.InstitutionCode))
            {
                ModelState.AddModelError("InstitutionCode", "機構代碼已存在，請使用其他代碼。");
            }

            // 檢查機構 Email 是否已存在
            if (await _context.Institutions.AsNoTracking()
                .AnyAsync(i => i.Email == vm.InstitutionEmail))
            {
                ModelState.AddModelError("InstitutionEmail", "機構 Email 已存在，請使用其他 Email。");
            }

            // 檢查帳號 Email 是否已存在
            if (await _context.Users.AsNoTracking()
                .AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "帳號 Email 已註冊，請使用其他 Email。");
            }

            if (string.IsNullOrWhiteSpace(vm.ImageName) || string.IsNullOrWhiteSpace(vm.ImageUrl))
            {
                ModelState.AddModelError(string.Empty, "請先上傳圖片，再提交註冊。");
                return View(vm);
            }

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var orgOwnerRoleId = await _context.Roles
        .Where(r => r.RoleCode == "OrgOwner")
        .Select(r => r.Id)
        .FirstOrDefaultAsync();

            if (orgOwnerRoleId == 0)
            {
                ModelState.AddModelError(string.Empty, "系統尚未設定預設身分。請聯繫管理員。");
                return View(vm);
            }




            //使用交易來確保資料一致性
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                
                using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                //建立機構
                var institution =  new Institution
                {
                    InstitutionCode = vm.InstitutionCode,
                    Name = vm.InstitutionName,
                    Email = vm.InstitutionEmail,
                    Phone = vm.InstitutionPhone,
                    PrincipalName = vm.PrincipalName,
                    PrincipalPhone = vm.PrincipalPhone,
                    CityId = vm.CityId,
                    TownshipId = vm.TownshipId,
                    Address = vm.Address,
                    Status = "Pending", // 設定狀態為審核中
                    IsPhysicalCheck= false, 
                    IsBanned = false, // 預設不被停權
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                //建立使用者
                var user = new User
                {
                    UserId = Guid.NewGuid().ToString("N"), // 使用 GUID 作為唯一識別碼
                    Name = vm.Name, 
                    Email = vm.Email,
                    Password = HashUtility.HashPassword(vm.Password), 
                    Status = "unverified", // 設定狀態為未驗證
                    AccountType = "Institution",
                    ConfirmCode = Guid.NewGuid().ToString("N"), // 生成確認碼
                    ConfirmCodeExpiresAt = DateTime.Now.AddHours(24), // 確認碼有效期為24小時
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now

                };

                _context.Institutions.Add(institution);
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // 先拿到 institution.Id 與 user.Id

                //建立機構與使用者關聯
                _context.InstitutionProfiles.Add(new InstitutionProfile
                {
                    UserId = user.Id,
                    InstitutionId = institution.Id,
                    IsResponsible = true, // 預設為機構系統負責人
                    Position = "機構系統負責人", // 預設職位為機構負責人
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now

                });

                //帳號身分
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    //RoleCode = "Institution", // 預設角色為機構使用者
                    RoleId = orgOwnerRoleId, // 預設角色為機構使用者
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });


                    _context.InstitutionAuditImages.Add(new InstitutionAuditImage
                    {
                        InstitutionId = institution.Id,
                        ImgName = vm.ImageName,     // 隱藏欄位傳上來的檔名
                        MimeType = vm.MimeType,     // 隱藏欄位傳上來的 MIME
                        CreatedAt = DateTime.Now
                        //FileId = vm.ImageUrl
                    });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

                
            });

            return RedirectToAction("RegisterSuccess");
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View();
        }






    }


}
