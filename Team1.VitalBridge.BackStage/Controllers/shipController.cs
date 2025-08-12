using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

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
			// 列出所有物流選項
			var shipsOption =  _context.Ships
                .AsNoTracking()
                .Include(s => s.OrderShipMethods)
                .Select( s=> new shipViewModel
                {
                    Id = s.Id,
                    ShipMethodName = s.ShipMethodName,
                    ShipCost = s.ShipCost,
                    IsActive = s.IsActive
				}
				)
                .ToList();
			return View(shipsOption);
        }

        [HttpGet]
		public IActionResult CreateShipMethod()
		{
			return View();
		}
		[HttpPost]
		public IActionResult CreateShipMethod(CreateShipMethodViewModel vm)
		{
			// 驗證模型狀態
            if(ModelState.IsValid == false) return View(vm);
            
			var ship = new Ship
			{
				ShipMethodName = vm.ShipMethodName,
				ShipCost = vm.ShipCost,
				IsActive = vm.IsActive
			};
			_context.Ships.Add(ship);
			_context.SaveChanges();

			return View();
		}




	}
}
