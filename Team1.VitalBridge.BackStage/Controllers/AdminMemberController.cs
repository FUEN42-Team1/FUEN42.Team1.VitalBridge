using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
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
        private readonly LocationService _locationService;

        public AdminMemberController(AppDbContext context, IConfiguration configuration, LocationService locationService)
        {
            this._context = context;
            this._configuration = configuration;
            this._locationService = locationService;
        }


        public IActionResult Index()
        {
            var viewModelList = _context.Users
                .Where(u => u.AccountType == "Member")
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,

                    CityId = u.MemberProfile.CityId,
                    TownshipId = u.MemberProfile.TownshipId,
                    Address = u.MemberProfile.Address,
                    CityName = _context.Citys
                        .Where(c => c.Id == u.MemberProfile.CityId)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "-",
                    TownshipName = _context.Townships
                        .Where(t => t.Id == u.MemberProfile.TownshipId)
                        .Select(t => t.Name)
                        .FirstOrDefault() ?? "-",

                    Status = u.Status,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,

                    RoleDisplay = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .AsNoTracking()
                .ToList();

            return View(viewModelList);
        }


        //編輯會員
        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId && u.AccountType == "Member")
                .Include(u => u.MemberProfile)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            var vm = new UserViewModel
            {
                Id = user.Id,
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                CityId = user.MemberProfile?.CityId,
                TownshipId = user.MemberProfile?.TownshipId,
                Address = user.MemberProfile?.Address,
                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles?.Select(ur => ur.Role.RoleCode).ToArray() ?? Array.Empty<string>()
            };

            // 不用塞城市、鄉鎮資料，全由前端 AJAX 載入
            ViewBag.AllRoles = _context.Roles
                .Where(r => r.RoleType == "Member")
                .Select(r => new SelectListItem { Value = r.RoleCode, Text = r.Name })
                .ToList();

            return PartialView("_EditMemberPartial", vm);
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel vm)
        {
            if (!ModelState.IsValid)
            {

                ViewBag.AllCities = _locationService.GetAllCities();
                ViewBag.AllTownships = _locationService.GetTownshipsByCityId(vm.CityId ?? 0);
                ViewBag.AllRoles = _context.Roles
                    .Where(r => r.RoleType == "Member")
                    .Select(r => new SelectListItem
                    {
                        Value = r.RoleCode,
                        Text = r.Name
                    }).ToList();

                //return View(vm);
                return PartialView("_EditMemberPartial", vm);
            }

            var user = await _context.Users
                .Include(u => u.MemberProfile)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == vm.UserId && u.AccountType == "Member");

            if (user == null) return NotFound();

            user.Name = vm.Name;
            user.Phone = vm.Phone;
            user.Status = vm.Status;
            user.UpdatedAt = DateTime.Now;

            if (user.MemberProfile != null)
            {
                user.MemberProfile.CityId = vm.CityId;
                user.MemberProfile.TownshipId = vm.TownshipId;
                user.MemberProfile.Address = vm.Address;
                user.MemberProfile.UpdatedAt = DateTime.Now;
            }

            var currentRoleCodes = user.UserRoles.Select(ur => ur.Role.RoleCode).ToList();
            var newRoleCodes = (vm.Roles ?? Array.Empty<string>()).ToList();

            var rolesToRemove = user.UserRoles.Where(ur => !newRoleCodes.Contains(ur.Role.RoleCode)).ToList();
            foreach (var ur in rolesToRemove)
                user.UserRoles.Remove(ur);

            var rolesToAdd = newRoleCodes.Except(currentRoleCodes).ToList();
            foreach (var roleCode in rolesToAdd)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleCode == roleCode && r.RoleType == "Member");
                if (role != null)
                {
                    user.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }
            }

            await _context.SaveChangesAsync();

            //return RedirectToAction("Index");
            return Json(new { success = true });
        }












        //詳細資訊
        public async Task<IActionResult> Detail(string userId)
        {
            var user = await _context.Users
                .Where(u => u.AccountType == "Member" && u.UserId == userId)
                .Include(u => u.MemberProfile)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();

            var names = _locationService.ResolveNames(
                user.MemberProfile?.CityId,
                user.MemberProfile?.TownshipId
            );

            var vm = new UserViewModel
            {
                Id = user.Id,
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,

                CityId = user.MemberProfile?.CityId,
                TownshipId = user.MemberProfile?.TownshipId,
                Address = user.MemberProfile?.Address,
                CityName = names.CityName ?? "-",
                TownshipName = names.TownshipName ?? "-",

                Status = user.Status,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,

                RoleDisplay = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new List<string>()
            };

            return PartialView("_UserDetailPartial", vm);
        }



        public IActionResult Delete(int id)
        {
            //刪除帳號 應該是不用
            return RedirectToAction("Index"); // 刪除後重定向到成員列表頁面
        }





    }
}
