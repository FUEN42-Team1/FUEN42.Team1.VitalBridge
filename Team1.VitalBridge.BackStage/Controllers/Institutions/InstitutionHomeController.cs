using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.BackStage.Controllers.Institutions
{
    [Authorize(AuthenticationSchemes = "InstitutionJwtScheme")]
    [Route("Institution/[controller]/[action]")]
    public class InstitutionHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
