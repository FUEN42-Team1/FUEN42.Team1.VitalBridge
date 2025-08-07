using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.Utilities;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminJwtScheme")]
    [Route("Admin/[controller]/[action]")]
    public class AdminAuthController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public AdminAuthController(AppDbContext context, IConfiguration configuration, JwtService jwtService)
        {
            this._context = context;
            this._configuration = configuration;
            this._jwtService = jwtService;

        }
        public IActionResult Index()
        {

            return View();
        }

        //登入
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {

            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            // 檢查使用者是否存在

            var adminUser = await _context.Users
               .Include(au => au.UserRoles) // 載入 AdminUsersRoles 集合
                   .ThenInclude(aur => aur.Role) // 從 AdminUsersRoles 進入，載入實際的 AdminRole
                       .ThenInclude(ar => ar.RolePermissions) // 從 AdminRole 進入，載入 AdminRolePermissions 集合 (假設有此中間表)
                           .ThenInclude(arp => arp.Permission) // 從 AdminRolePermissions 進入，載入實際的 AdminPermission
               .SingleOrDefaultAsync(au => au.Email == vm.Email);


            // 驗證使用者是否存在且密碼正確
            if (adminUser == null || !HashUtility.VerifyPassword(vm.Password, adminUser.Password))
            {
                ModelState.AddModelError("", "無效的電子郵件或密碼。");
                return View(vm);
            }

            // 檢查是否已驗證開通
            //if (adminUser.Status == "unverified")
            //{
            //    ModelState.AddModelError("", "您的帳號尚未啟用，請檢查電子郵件進行驗證。");
            //    return View(vm);
            //}

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
            new Claim(ClaimTypes.NameIdentifier, adminUser.UserId), // 使用者唯一ID
            new Claim(ClaimTypes.Email, adminUser.Email),
            new Claim(ClaimTypes.Name, adminUser.Name)
        };


            if (adminUser.UserRoles?.Any() == true)
            {
                // 使用 SelectMany 取得所有唯一的角色代碼
                var roleCodes = adminUser.UserRoles
                    .Where(r => r.Role != null && !string.IsNullOrEmpty(r.Role.RoleCode))
                    .Select(r => r.Role.RoleCode)
                    .Distinct()
                    .ToList();

                // 將所有角色代碼轉換為 Claim 物件並添加到 claims 列表中
                claims.AddRange(roleCodes.Select(roleCode => new Claim(ClaimTypes.Role, roleCode)));

                // 使用 SelectMany 取得所有唯一的權限代碼
                var permissionCodes = adminUser.UserRoles
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
            var jwtToken = _jwtService.GenerateToken(claims, expiresMinutes, "AdminAudience");// 生成 JWT Token
            _jwtService.SetTokenCookie(jwtToken, expiresMinutes, Response, "admin_auth_token", "/Admin");// 將 JWT 存入 HttpOnly Cookie

            return RedirectToAction("Index", "Admin");

        }


        //登出
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // 清除 JWT Cookie
            _jwtService.ClearTokenCookie(Response, "admin_auth_token", "/Admin");
            // 重定向到登入頁面
            return RedirectToAction("Login", "AdminAuth");
        }

        





    }
}
