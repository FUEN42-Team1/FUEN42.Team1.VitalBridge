using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public InstitutionAuthController(AppDbContext context, IConfiguration configuration, JwtService jwtService)
        {
            this._context = context;
            this._configuration = configuration;
            this._jwtService = jwtService;

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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(InstitutionLoginViewModel vm)
        {

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            //先檢查機構狀態
            var inst = await _context.Institutions
                   .AsNoTracking()
                   .SingleOrDefaultAsync(x => x.InstitutionCode == vm.InstitutionCode);

            if (inst == null)
            {
                ModelState.AddModelError(string.Empty, "機構代碼、帳號或密碼不正確");
                return View(vm);
            }

            if (inst.IsBanned) {
                ModelState.AddModelError(string.Empty, "機構已停權，請聯絡管理員");
                return View(vm);
            }

            if (inst.Status == "Pending") {
                ModelState.AddModelError(string.Empty, "機構審核中");
                return View(vm);
            }


            //接著處理帳號


            
            








            // 檢查使用者是否存在

            var InstitutionUser = await _context.Users
               .AsNoTracking()
               .AsSplitQuery()
               .Include(au => au.UserRoles)
                   .ThenInclude(aur => aur.Role) 
                       .ThenInclude(ar => ar.RolePermissions) 
                           .ThenInclude(arp => arp.Permission)
                 .Include(au => au.InstitutionProfile) 
                 .ThenInclude(ip => ip.Institution) 
               .SingleOrDefaultAsync(au => au.Email == vm.Email);


            // 驗證使用者是否存在且密碼正確
            if (InstitutionUser == null || !HashUtility.VerifyPassword(vm.Password, InstitutionUser.Password))
            {
                ModelState.AddModelError("", "無效的電子郵件或密碼。");
                return View(vm);
            }

            //檢查是否已驗證開通
            if (InstitutionUser.Status == "unverified")
            {
                ModelState.AddModelError("", "您的帳號尚未啟用，請檢查電子郵件進行驗證。");
                return View(vm);
            }

            //if (!adminUser.AdminUsersRoles.Any(aur => aur.Role != null &&
            //                          (aur.Role.RoleCode.Trim().Equals("ADMINISTRATOR", StringComparison.OrdinalIgnoreCase) ||
            //                           aur.Role.RoleCode.Trim().Equals("DATA_BROWSER", StringComparison.OrdinalIgnoreCase))))
            //{
            //    ModelState.AddModelError("", "此帳號無權登入管理後台。");
            //    return View(vm);
            //}



            // 建立 Claims 列表 
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, InstitutionUser.UserId), // 使用者唯一ID
            new Claim(ClaimTypes.Email, InstitutionUser.Email),
            new Claim(ClaimTypes.Name, InstitutionUser.Name)
        };


            if (InstitutionUser.UserRoles?.Any() == true)
            {
                // 使用 SelectMany 取得所有唯一的角色代碼
                var roleCodes = InstitutionUser.UserRoles
                    .Where(r => r.Role != null && !string.IsNullOrEmpty(r.Role.RoleCode))
                    .Select(r => r.Role.RoleCode)
                    .Distinct()
                    .ToList();

                // 將所有角色代碼轉換為 Claim 物件並添加到 claims 列表中
                claims.AddRange(roleCodes.Select(roleCode => new Claim(ClaimTypes.Role, roleCode)));

                // 使用 SelectMany 取得所有唯一的權限代碼
                var permissionCodes = InstitutionUser.UserRoles
                    .Where(r => r.Role?.RolePermissions?.Any() == true)
                    .SelectMany(r => r.Role.RolePermissions)
                    .Where(p => p.Permission != null && !string.IsNullOrEmpty(p.Permission.PermissionCode))
                    .Select(p => p.Permission.PermissionCode)
                    .Distinct()
                    .ToList();

                // 將所有權限代碼轉換為 Claim 物件並添加到 claims 列表中
                claims.AddRange(permissionCodes.Select(permissionCode => new Claim("Permission", permissionCode)));
            }


            // 生成 JWT Token
            // 確保 Expires 時間一致
            var expiresMinutes = double.Parse(_configuration["JwtSettings:ExpirationMinutes"]);
            var jwtToken = _jwtService.GenerateToken(claims, expiresMinutes, "InstitutionJwtScheme");// 生成 JWT Token
            _jwtService.SetTokenCookie(jwtToken, expiresMinutes, Response, "institution_auth_token", "/Institution");// 將 JWT 存入 HttpOnly Cookie

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
        public async Task<IActionResult> Register(InstitutionRegisterLoginViewModel vm) { 
        

            return View(vm);
        }




    }


}
