using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
	public class ProductsController : Controller
	{
		private readonly AppDbContext _context;

		public ProductsController(AppDbContext context)
		{
			this._context = context;
		}
		public IActionResult Index()
		{
			var data = _context.Products
				.AsNoTracking()
				.Select(p => p.ToIndexVm())
				.ToList();
			return View(data);
		}
	}
}
