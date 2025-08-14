using Microsoft.AspNetCore.Mvc;

namespace Team1.VitalBridge.BackStage.Controllers.Institutions
{
    public class InstitutionOwnershipController : Controller
    {


        //機構認領

        public IActionResult Index()
        {
            return View();
        }

        //機構認領列表
        public IActionResult SearchList() {

            //顯示機構名稱、地址、是否有人管理、

            return View();
        }



    }
}
