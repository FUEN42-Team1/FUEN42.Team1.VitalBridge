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
        })
        .ToList();

            return View(roles);
        }

        //新增身分
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RoleViewModel vm)
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
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Roles.Add(newRole);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "身分新增成功！";
            
            return RedirectToAction("Index");
        }

        //編輯身分
        public IActionResult Edit(string roleCode) {

            var role = _context.Roles.FirstOrDefault(r => r.RoleCode == roleCode);
            if (role == null)
                return NotFound();

            if (role.IsSystemDefault) {
                
                return RedirectToAction("Index");
            }

            var vm = new RoleViewModel
            {
                RoleCode = role.RoleCode,
                Name = role.Name,
                Info = role.Info,
                IsActive = role.IsActive,
                RoleType = Enum.TryParse<RoleTypeItem>(role.RoleType, out var type)
                            ? type
                            : RoleTypeItem.Member // 預設 fallback
            };

            return View(vm);
        }

        [HttpPost]
        public IActionResult Edit(RoleViewModel vm) {

            if (!ModelState.IsValid)
            {
                return View(vm);
            }



            var role = _context.Roles.FirstOrDefault(r => r.RoleCode == vm.RoleCode);
            if (role == null)
                return NotFound();

            if (role.IsSystemDefault)
            {

                return RedirectToAction("Index");
            }

            role.Name = vm.Name;
            role.Info = vm.Info;
            role.IsActive = vm.IsActive;
            role.RoleType = vm.RoleType.ToString(); // ← 儲存為文字
            role.UpdatedAt = DateTime.Now;

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
