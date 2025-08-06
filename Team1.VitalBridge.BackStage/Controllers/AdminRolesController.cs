using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.ViewModels;

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
            Info = r.Info,
            isActive = r.IsActive,
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
            return RedirectToAction("Index");
        }

        //編輯身分

        //刪除身分(低低優先)



    }
}
