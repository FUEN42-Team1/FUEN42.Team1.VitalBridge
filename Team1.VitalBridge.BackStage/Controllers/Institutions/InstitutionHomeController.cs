using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.BackStage.Controllers.Institutions
{
    [Authorize(AuthenticationSchemes = "InstitutionJwtScheme")]
    [Route("Institution/[controller]/[action]")]
    public class InstitutionHomeController : Controller
    {
        //機構後臺主頁
        public IActionResult Index()
        {
            return View();
        }

        //審核駁回重新填寫流程
        public IActionResult ReApply()
        {
            return View();
        }

        //機構基本資料


        //機構員工管理(自己的)




    }
}
