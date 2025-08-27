using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using static Team1.VitalBridge.BackStage.Models.ViewModels.RoleViewModel;

namespace Team1.VitalBridge.BackStage.Controllers
{


    [Authorize(AuthenticationSchemes = "AdminJwtScheme")]
    [Route("Admin/[controller]/[action]")]
    public class AdminRolesController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public AdminRolesController(AppDbContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;
        }




        public IActionResult Index()
        {
            var roles = _context.Roles
                .Where(r => r.RoleType == "Admin" || r.RoleType == "Member")
                .OrderBy(r => r.CreatedAt)
                .Select(r => new RoleViewModel
                {
                    RoleCode = r.RoleCode,
                    Name = r.Name,
                    RoleTypeShowName = r.RoleType,
                    Info = r.Info,
                    IsActive = r.IsActive,
                    IsSystemDefault = r.IsSystemDefault,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }
            ).ToList();

            return View(roles);
        }

        //新增身分
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RoleCreateViewModel vm)
        {
            // 在這裡處理新增身分的邏輯
            // 例如，將 roleName 儲存到資料庫中
            // 假設新增成功後，重定向到身分列表頁面

            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var newRole = new Role
            {
                RoleCode = vm.RoleCode,
                RoleType=vm.RoleTypeName,
                Name = vm.Name,
                Info = vm.Info,
                IsActive = vm.IsActive,
                IsSystemDefault = false, // 新增的身分預設不是系統預設
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Roles.Add(newRole);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "身分新增成功！";
            
            return RedirectToAction("Index");
        }

        // 編輯身分 GET
        public IActionResult Edit(string roleCode)
        {
            var role = _context.Roles.FirstOrDefault(r => r.RoleCode == roleCode);
            if (role == null)
                return NotFound();

            if (role.IsSystemDefault)
                return RedirectToAction("Index");

            var roleId = role.Id;

            // 取得所有權限
            var allPermissions = _context.Permissions.ToList();

            // 取得此身分已選的權限
            var selectedIds = _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToList();

            var vm = new RoleViewModel
            {
                RoleCode = role.RoleCode,
                Name = role.Name,
                Info = role.Info,
                IsActive = role.IsActive,
                RoleType = Enum.TryParse<RoleTypeItem>(role.RoleType, out var type)
                            ? type
                            : RoleTypeItem.Member,
                //AllPermissions = allPermissions,
                //SelectedPermissionIds = selectedIds
            };

            return View(vm);
        }

        // 編輯身分 POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RoleViewModel vm)
        {
            //if (!ModelState.IsValid)
            //{
            //    // 重新載入權限資料，避免回傳 View 時權限清單為空
            //    vm.AllPermissions = _context.Permissions.ToList();
            //    return View(vm);
            //}

            var role = _context.Roles.FirstOrDefault(r => r.RoleCode == vm.RoleCode);
            if (role == null)
                return NotFound();

            if (role.IsSystemDefault)
                return RedirectToAction("Index");

            var roleId = role.Id;

            // 更新身分基本資料
            role.Name = vm.Name;
            role.Info = vm.Info;
            role.IsActive = vm.IsActive;
            role.RoleType = vm.RoleType.ToString();
            role.UpdatedAt = DateTime.Now;

            // 更新權限設定
            //var oldPermissions = _context.RolePermissions.Where(rp => rp.RoleId == roleId);
            //_context.RolePermissions.RemoveRange(oldPermissions);

            //if (vm.SelectedPermissionIds != null)
            //{
            //    foreach (var pid in vm.SelectedPermissionIds)
            //    {
            //        _context.RolePermissions.Add(new RolePermission
            //        {
            //            RoleId = roleId,
            //            PermissionId = pid
            //        });
            //    }
            //}

            _context.SaveChanges();

            TempData["SuccessMessage"] = "身分修改成功！";
            return RedirectToAction("Index");
        }



        //啟用/停用身分
        [HttpPost]
        public IActionResult ToggleStatus(string roleCode)
        {
            if (string.IsNullOrEmpty(roleCode))
            {
                return BadRequest("Role code is required.");
            }

            var role = _context.Roles.FirstOrDefault(r => r.RoleCode == roleCode);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            // 切換狀態
            role.IsActive = !role.IsActive;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                isActive = role.IsActive // ✅ 回傳目前的狀態
            });

        }

        //刪除身分(低低優先)
        public IActionResult Delete()
        {
            //刪除前需要將所有使用此身分的使用者身分清除
            //並設為Member身分



            return View();
        }




    }
}
