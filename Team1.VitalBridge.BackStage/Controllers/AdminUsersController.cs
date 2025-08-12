using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.Utilities;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Team1.VitalBridge.BackStage.Models.ViewModels.Admin;

namespace Team1.VitalBridge.BackStage.Controllers
{

    [Authorize(AuthenticationSchemes = "AdminJwtScheme")]
    [Route("Admin/[controller]/[action]")]

    public class AdminUsersController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        //管理會員相關
        public AdminUsersController(AppDbContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;

        }





        //取得所有管理員使用者
        //這個方法會取得所有的管理員使用者
        public async Task<IActionResult> Index()
        {
            var adminlist = await _context.Users
                .AsNoTracking()
                .Where(u => u.AccountType == "Admin")
                .Include(u => u.AdminProfile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new AdminUserListVM
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    Status = u.Status,
                    Note = u.AdminProfile != null ? u.AdminProfile.Note : null,
                    LastLoginAt = u.LastLoginAt,
                    LastAdminActionAt = u.AdminProfile != null
                                        ? u.AdminProfile.LastAdminActionAt
                                        : null,
                    Roles = u.UserRoles != null
                        ? u.UserRoles
                            .Where(ur => ur.Role != null)
                            .Select(ur => ur.Role.Name)
                            .ToArray()
                        : Array.Empty<string>()
                })
                .OrderByDescending(x => x.LastAdminActionAt ?? x.LastLoginAt)
                .ToListAsync();

            return View(adminlist);
        }

        //取得單一管理員使用者的詳細資料
        //這個方法會取得指定管理員使用者的詳細資料
        [HttpGet]
        public async Task<IActionResult> Details(string userId)
        {
            var adminUser = await _context.Users
                .AsNoTracking()
                .Include(u => u.AdminProfile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.AccountType == "Admin");
            if (adminUser == null) return NotFound();
            var vm = new AdminUserDetailsVM
            {
                UserId = adminUser.UserId,
                Name = adminUser.Name,
                Email = adminUser.Email,
                Phone = adminUser.Phone,
                Status = adminUser.Status,
                Note = adminUser.AdminProfile != null ? adminUser.AdminProfile.Note : null,
                CreatedAt = adminUser.CreatedAt,
                UpdatedAt = adminUser.UpdatedAt,
                LastLoginAt = adminUser.LastLoginAt,
                LastAdminActionAt = adminUser.AdminProfile != null ? adminUser.AdminProfile.LastAdminActionAt : null,
                FailedLoginCount = adminUser.FailedLoginCount,
                LockedUntil = adminUser.LockedUntil,
                Roles = adminUser.UserRoles != null
                    ? adminUser.UserRoles
                        .Where(ur => ur.Role != null)
                        .Select(ur => ur.Role.Name)
                        .ToArray()
                    : Array.Empty<string>()
            };
            return View(vm);
        }
































        //重設密碼
        //指定管理員使用者後，啟動重設密碼流程
        //先是寄送重設密碼的郵件，然後使用者點擊郵件中的連結來重設密碼
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendResetPasswordLink(string AdminUserId)
        {
            var adminUser = await _context.Users
        .FirstOrDefaultAsync(u => u.UserId == AdminUserId); 
            if (adminUser == null) return NotFound();

            // 建立 token
            var token = Guid.NewGuid().ToString();
            var expiration = DateTime.UtcNow.AddMinutes(10); // 10 分鐘後過期
            adminUser.ResetPasswordConfirmCode = token;
            adminUser.ResetPasswordConfirmCodeExpiresAt = expiration;

            await _context.SaveChangesAsync();

            // 組合 Reset 密碼連結
            var resetLink = Url.Action("ResetPassword", "AdminUsers", new {uid = AdminUserId, code = token }, Request.Scheme);
            //測試期間用debug console
            Debug.WriteLine($"DEBUG INFO: Password reset link for {adminUser.Email}: {resetLink}");

            // 寄送 email
            //await _emailService.SendAsync(adminUser.Email, "重設您的管理員密碼", $"請點擊下方連結：\n{resetLink}");

            return Ok("重設密碼信件已寄出");
            //return View("Test");
        }

        //重設密碼頁面
        //使用者點擊郵件中的連結後，會導向到這個頁面來重設密碼
        [AllowAnonymous]
        //[HttpGet("ResetPassword/{uid}/{code}")]
        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string uid, string code)
        {
            // 檢查使用者是否存在
            var adminUser = _context.Users.FirstOrDefault(u => u.UserId == uid && u.ResetPasswordConfirmCode == code);
            if (adminUser == null || adminUser.ResetPasswordConfirmCodeExpiresAt < DateTime.UtcNow)
            {
                return NotFound("無效的重設密碼連結或已過期");
            }
            // 返回重設密碼視圖
            var vm = new ResetPasswordViewModel
            {
                uid = uid,
                Code = code
            };
            return View(vm);
        }

        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            // 檢查使用者是否存在

            if (!ModelState.IsValid)
                return View(vm);

            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == vm.uid && u.ResetPasswordConfirmCode == vm.Code);

            if (adminUser == null || adminUser.ResetPasswordConfirmCodeExpiresAt < DateTime.UtcNow)
            {
                return NotFound("無效的重設密碼連結或已過期");
            }

            // 更新密碼
            adminUser.Password = HashUtility.HashPassword(vm.NewPassword);
            adminUser.ResetPasswordConfirmCode = null; // 清除重設密碼的 token
            adminUser.ResetPasswordConfirmCodeExpiresAt = null; // 清除過期時間
            await _context.SaveChangesAsync();
            return RedirectToAction("Login", "AdminAuth"); // 重定向到登入頁面
        }







        //新增使用者(低順位)
        public IActionResult CreateAdmin()
        {
            return View();
        }

    }
}
