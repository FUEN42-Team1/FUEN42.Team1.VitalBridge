using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class shipController : Controller
    {
        private readonly AppDbContext _context;

        public shipController(AppDbContext context) {
            this._context=context;
        }

        //Get: ship
        public IActionResult Index()
        {
            return View();
        }
    }
}
