using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Linq;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
	public class ProductCategoryController : Controller
	{
		


		// Get:ProductCategory
		[HttpGet]
		// 把要把類別頁面都叫出來
		public  IActionResult Index()
		{
			
			return View();
		}

		/*
		// Get:ProductCategory/Create
		[HttpGet]
		// 這是顯示新增表單
		public async Task<IActionResult> Create()
		{
			var vm = new ProductCategoryFormViewModel();
			await LoadParentOptions(vm);
			return View(vm);
		}


		

		//Post :ProductCategory/CreateProductCategory
		[HttpPost]
		// 這是傳輸新增表單資料
		public async Task<IActionResult> Create(ProductCategoryFormViewModel vm)
		{
			// 確認驗證
			if (ModelState.IsValid == false)
			{
				// 驗證失敗時，重新載入下拉選單選項
				await LoadParentOptions(vm);
				return View(vm);

			}

			// 驗證後，將vm取得的資料，給EFmodel
			var categories = new Category
			{
				FatherId = vm.FatherId,
				Name = vm.Name,
				//BannerImageUrl = vm.BannerImageUrl,
				IsActive = vm.IsActive

			};

			
			//_context.Add(categories);
			//await _context.SaveChangesAsync();
			//return RedirectToAction(nameof(Index));
		}

		// 共用方法 載入下拉式選單
		

		private async Task LoadParentOptions(ProductCategoryFormViewModel vm)
		{
			// 1. 從資料庫查詢所有啟用的類別
			var categories = await _context.Categories
				.Where(c => c.IsActive).ToListAsync();

			// 2. 初始化下拉選單清單，並加入「無父類別」選項
			vm.ParentOptions = new List<SelectListItem>
			{
				new SelectListItem("--無上層類別--","") 
				//Text="-- 無上層類別 --", Value=""
			};

			// 3. 遍歷每個類別，轉換成下拉選單格式
			foreach (var category in categories)
			{
				vm.ParentOptions.Add(new SelectListItem(category.Name, category.Id.ToString()));
			}

		}

		*/
	}
}
