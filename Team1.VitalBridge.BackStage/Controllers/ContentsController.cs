using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ContentsIndex()
        {
            return View();
        }

        public IActionResult ContentTypeCategoryPairsIndex()
        {
            return View();
        }
    }
}
