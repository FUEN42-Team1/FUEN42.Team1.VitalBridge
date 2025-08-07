using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.BackStage.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminJwtScheme")]
    [Route("Admin/[controller]/[action]")]
    public class AdminInstitutionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }



    }
}
