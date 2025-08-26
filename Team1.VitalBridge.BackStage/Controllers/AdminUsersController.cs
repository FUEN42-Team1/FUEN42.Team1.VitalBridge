using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                    LockedUntil = u.LockedUntil,
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





        //編輯管理員身分
        //需要超管
        public async Task<IActionResult> EditRoles(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return BadRequest("缺少 UserId");

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.AccountType == "Admin");

            if (user == null) return NotFound("找不到此管理員帳號");

            // 只抓「管理員類型」且啟用中的角色清單
            var allAdminRoles = await _context.Roles
                .AsNoTracking()
                .Where(r => r.RoleType == "Admin" && r.IsActive)
                .Select(r => new RoleOptionVM { Code = r.RoleCode, Name = r.Name })
                .OrderBy(r => r.Code)
                .ToListAsync();

            var selected = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.RoleCode)
                .ToList();

            var vm = new AdminEditRolesVM
            {
                UserId = user.UserId,
                AllRoles = allAdminRoles,
                SelectedRoles = selected
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(AdminEditRolesVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.UserId))
            {
                ModelState.AddModelError(nameof(vm.UserId), "UserId 必填");
            }
            if (!ModelState.IsValid) return View(vm);

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == vm.UserId && u.AccountType == "Admin");

            if (user == null) return NotFound("找不到此管理員帳號");

            // 目標集合（前端送回的勾選）
            var targetCodes = (vm.SelectedRoles ?? new List<string>()).ToHashSet();

            // 目前擁有
            var currentCodes = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.RoleCode)
                .ToHashSet();

            // 差異計算
            var codesToAdd = targetCodes.Except(currentCodes).ToList();
            var codesToRemove = currentCodes.Except(targetCodes).ToList();

            // 找出要新增/移除對應的 RoleId
            var addRoleIds = await _context.Roles
                .Where(r => r.RoleType == "Admin" && r.IsActive && codesToAdd.Contains(r.RoleCode))
                .Select(r => r.Id)
                .ToListAsync();

            var removeRoleIds = await _context.Roles
                .Where(r => r.RoleType == "Admin" && codesToRemove.Contains(r.RoleCode))
                .Select(r => r.Id)
                .ToListAsync();

            // 移除多餘
            if (removeRoleIds.Count > 0)
            {
                var toRemove = user.UserRoles.Where(ur => removeRoleIds.Contains(ur.RoleId)).ToList();
                _context.UserRoles.RemoveRange(toRemove);
            }

            // 新增缺少
            foreach (var rid in addRoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = rid
                });
            }

            await _context.SaveChangesAsync();

            // 依你路由調整：回詳細頁或清單頁
            return RedirectToAction("Details", new { userId = user.UserId });
        }




        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(AdminEditVM vm)
        {
            if (!ModelState.IsValid)
            {
                // 開窗旗標與回填
                TempData["OpenEditModal"] = "1";
                TempData["EditVM_Name"] = vm.Name ?? "";
                TempData["EditVM_Phone"] = vm.Phone ?? "";
                TempData["EditVM_Note"] = vm.Note ?? "";

                // （除錯期可把錯誤也帶回頁面看）
                // TempData["Edit_Errors"] = string.Join(" | ",
                //     ModelState.Where(x => x.Value?.Errors.Count > 0)
                //               .Select(x => $"{x.Key}:{string.Join(",", x.Value!.Errors.Select(e => e.ErrorMessage))}"));

                return RedirectToAction(nameof(Details), new { userId = vm.UserId });
            }

            var user = await _context.Users
                .Include(u => u.AdminProfile)
                .FirstOrDefaultAsync(u => u.UserId == vm.UserId && u.AccountType == "Admin");
            if (user == null) return NotFound();

            user.Name = vm.Name.Trim();
            user.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();

            if (user.AdminProfile == null)
                user.AdminProfile = new AdminProfile { UserId = user.Id };
            user.AdminProfile.Note = string.IsNullOrWhiteSpace(vm.Note) ? null : vm.Note.Trim();

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "已更新管理員基本資料。";
            return RedirectToAction(nameof(Details), new { userId = vm.UserId });
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


        //重設帳號鎖定
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetLock(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId && u.AccountType == "Admin");
            if (user == null) return NotFound();

            // 若已經沒鎖，直接回到詳細頁
            if (!user.LockedUntil.HasValue || user.LockedUntil <= DateTime.UtcNow)
            {
                TempData["Info"] = "帳號目前未鎖定，不需解鎖。";
                return RedirectToAction("Details", new { AdminUserId = userId });
            }

            user.FailedLoginCount = 0;
            user.LockedUntil = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // 紀錄稽核軌跡（建議）
            //_audit.Log(User, "AdminUserUnlock", new { TargetUserId = user.UserId });

            TempData["Success"] = "已解除鎖定並清除累計登入失敗次數。";
            return RedirectToAction("Details", new { userId = userId });
        }




        //新增使用者(低順位)
        public IActionResult CreateAdmin()
        {
            return View();
        }

    }
}
