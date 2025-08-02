using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly JwtService _jwtService;

        public AdminMemberController(AppDbContext context, IConfiguration configuration, JwtService jwtService)
        {
            this._context = context;
            this._configuration = configuration;
            this._jwtService = jwtService;

        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserList()
        {
            var userdata = _context.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
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
                    LastLoginAt = u.LastLoginAt
                })
                .ToList();

            return View("UserList", userdata);
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

        public IActionResult Edit(int id)
        {
            // 根據 ID 獲取成員資料並返回編輯視圖
            // 這裡可以使用服務或資料庫上下文來獲取成員資料
            return View();
        }

        //詳細資訊
        public IActionResult Detail(string userId) {


            return View();
        }


        public IActionResult Delete(int id)
        {
            // 根據 ID 刪除成員資料
            // 這裡可以使用服務或資料庫上下文來刪除成員資料
            return RedirectToAction("Index"); // 刪除後重定向到成員列表頁面
        }



    }
}
