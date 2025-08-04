using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
	public class ProductCategoryController : Controller
	{
		private readonly AppDbContext _context;

		public ProductCategoryController(AppDbContext contexxt)
		{
			this._context = contexxt;
		}
		// 把要把類別頁面叫出來
		public IActionResult Index()
		{
			// 1. 取得所有類別資料

			var categories = _context.Categories
				.ToList();

			// 2. 轉換成ViewModel並計算層級

			var categoryTreelist = new List<ProductCategoryTreeViewModel>();

			// 3. 先處理第一層（ParentId == null）
			var rootCategories = categories
				.Where(c => c.FatherId == null).ToList();
			foreach(var root in rootCategories)
			{
				categoryTreelist.Add(new ProductCategoryTreeViewModel
				{
					Id = root.Id,
					name= root.Name,
					FatherId = root.FatherId,
					IsActive = root.IsActive,
					Level=0

				});

				// 4. 處理子類別
				// 在categories 裡面 篩選出 FatherId 已經有填的，也就是上面的root.Id
				// 因為root.id 代表已經是父類別，這邊就是在找所有root的子類別
				var children = categories.Where (c =>c.FatherId ==root.Id).ToList();
				foreach (var child in children)
				{
					categoryTreelist.Add(new ProductCategoryTreeViewModel
					{
						Id = child.Id,
						name = child.Name,
						FatherId = child.FatherId,
						IsActive = child.IsActive,
						Level = 1

					});

					//5. 第三層
					var childrenthird = categories.Where(c => c.FatherId == child.Id).ToList();
					foreach (var childthree in childrenthird)
					{
						categoryTreelist.Add(new ProductCategoryTreeViewModel
						{
							Id = childthree.Id,
							name = childthree.Name,
							FatherId = childthree.FatherId,
							IsActive = childthree.IsActive,
							Level = 2

						});

					}

				}



			}





			return View(categoryTreelist);
		}




	}
}
