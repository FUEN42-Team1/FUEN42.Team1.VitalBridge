using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.Frontend.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
