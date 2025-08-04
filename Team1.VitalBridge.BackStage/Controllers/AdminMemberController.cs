using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminJwtScheme")]
    [Route("Admin/[controller]/[action]")]
    public class AdminMemberController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public AdminMemberController(AppDbContext context, IConfiguration configuration)
        {
            this._context = context;
            this._configuration = configuration;

        }


        public IActionResult Index()
        {
            var userdata = _context.Users
    .Select(u => new UserViewModel
    {
        //Id = u.Id,
        UserId = u.UserId,
        Name = u.Name,
        Email = u.Email,
        Phone = u.Phone,
        CityId = u.CityId,
        //CityName = u.City?.Name, // 假設有 City 導覽屬性
        TownshipId = u.TownshipId,
        //TownshipName = u.Township?.Name, // 假設有 Township 導覽屬性
        Address = u.Address,
        Status = u.Status,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt,
        LastLoginAt = u.LastLoginAt,
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
    })
    .ToList();

            return View("UserList", userdata);
        }

        public IActionResult UserList()
        {
            //        var userdata = _context.Users
            //.Select(u => new UserViewModel
            //{
            //    //Id = u.Id,
            //    UserId = u.UserId,
            //    Name = u.Name,
            //    Email = u.Email,
            //    Phone = u.Phone,
            //    CityId = u.CityId,
            //    //CityName = u.City?.Name, // 假設有 City 導覽屬性
            //    TownshipId = u.TownshipId,
            //    //TownshipName = u.Township?.Name, // 假設有 Township 導覽屬性
            //    Address = u.Address,
            //    Status = u.Status,
            //    CreatedAt = u.CreatedAt,
            //    UpdatedAt = u.UpdatedAt,
            //    LastLoginAt = u.LastLoginAt,
            //    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            //})
            //.ToList();

            //        return View("UserList", userdata);
            return View();
        }



        // 這裡可以添加其他方法，例如新增、編輯、刪除成員等
        // 這些方法可以使用 [HttpGet] 或 [HttpPost] 特性來區分 GET 和 POST 請求
        // 例如：
        [HttpGet]
        public IActionResult Create()
        {
            // 返回創建成員的視圖
            return View();
        }


        //編輯使用者資料
        public IActionResult Edit(string userId)
        {
            var user = _context.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }


            var vm = new UserViewModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                CityId = user.CityId,
                TownshipId = user.TownshipId,
                Address = user.Address,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                SelectedRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList()
            };

            ViewBag.AllRoles = _context.Roles
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                .ToList();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AllRoles = _context.Roles
                    .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                    .ToList();
                return View(vm);
            }
            // 更新使用者資料
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == vm.UserId);
            if (user == null)
            {
                return NotFound();
            }
            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.CityId = vm.CityId;
            user.TownshipId = vm.TownshipId;
            user.Address = vm.Address;
            user.Status = vm.Status;
            user.UpdatedAt = DateTime.Now;

            var selectedRoleIds = vm.SelectedRoleIds.ToHashSet();
            var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();

            // 移除未選擇的角色
            var rolesToRemove = user.UserRoles
                .Where(ur => !selectedRoleIds.Contains(ur.RoleId))
                .ToList(); // 必須 ToList() 才能在迴圈中修改原集合

            foreach (var role in rolesToRemove)
            {
                user.UserRoles.Remove(role);
            }

            // 新增新的角色（避免重複）
            var toAdd = selectedRoleIds.Except(currentRoleIds).ToList();
            foreach (var roleId in toAdd)
            {
                user.UserRoles.Add(new UserRole
                {
                    UserId = user.Id, // ✅ 正確對應主鍵 Id
                    RoleId = roleId
                });
            }




            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("UserList");
        }

        //詳細資訊
        public async Task<IActionResult> Detail(string userId)
        {

            var vm = await _context.Users
       .Where(u => u.UserId == userId)
       .Select(u => new UserViewModel
       {
           //Id = u.Id,
           UserId = u.UserId,
           Name = u.Name,
           Email = u.Email,
           Phone = u.Phone,
           CityId = u.CityId,
           TownshipId = u.TownshipId,
           Address = u.Address,
           Status = u.Status,
           CreatedAt = u.CreatedAt,
           UpdatedAt = u.UpdatedAt,
           LastLoginAt = u.LastLoginAt,
           Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
       })
       .FirstOrDefaultAsync();

            if (vm == null)
            {
                return NotFound();
            }

            //return View(vm);
            return PartialView("_UserDetailPartial", vm);
        }


        public IActionResult Delete(int id)
        {
            //刪除帳號 應該是不用
            return RedirectToAction("Index"); // 刪除後重定向到成員列表頁面
        }



    }
}
