using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class AdminInstitutionUsersController : Controller
    {


        private readonly AppDbContext _context;

        public AdminInstitutionUsersController(AppDbContext context)
        {
            _context = context;
        }


        //機構相關員工
        // 機構相關員工清單
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _context.Users
                .AsNoTracking()
                .Include(u => u.InstitutionProfile)               // 使用者的機構業務資料
                    .ThenInclude(p => p.Institution)              // 機構
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Where(u => u.AccountType == "Institution")   // 依你實際條件
                .OrderBy(u => u.Name)
                .Select(u => new AdminInstitutionUserListItemVM
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    InstitutionId = u.InstitutionProfile != null ? u.InstitutionProfile.InstitutionId : (int?)null,
                    InstitutionCode = u.InstitutionProfile != null ? u.InstitutionProfile.Institution!.InstitutionCode : null,
                    InstitutionName = u.InstitutionProfile != null ? u.InstitutionProfile.Institution!.Name : null,
                    Status = u.Status,
                    LastLoginAt = u.LastLoginAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToArray()
                })
                .ToListAsync();

            return View(list); // 對應下面的 View
        }



        public IActionResult Create()
        {
            //新增機構相關員工
            return View();
        }

        public IActionResult Edit(string userId)
        {
            //編輯機構相關員工

            return View();
        }

        public IActionResult Delete(string userId)
        {
            //刪除機構相關員工

            return View();
        }

        public IActionResult Details(string userId)
        {
            //顯示機構相關員工詳細資料

            return View();
        }

        
        

    }
}
