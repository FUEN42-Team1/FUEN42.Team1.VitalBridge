using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class FeatureServicesController : Controller
    {
        private readonly AppDbContext _context;

        public FeatureServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FeatureServices
        public IActionResult Index()
        {
            // 此處假設您在資料庫中存了圖片路徑
            var featureServices = _context.FeatureServices.ToList();
            return View(featureServices);
        }

        // GET: FeatureServices/Create
        public IActionResult Create()
        {
            return View();
        }



    }
}