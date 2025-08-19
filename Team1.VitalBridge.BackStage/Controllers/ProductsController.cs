using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Team1.VitalBridge.BackStage.Controllers
{
	public class ProductsController : Controller
	{
		private readonly AppDbContext _context;

		public ProductsController(AppDbContext context)
		{
			this._context = context;
		}
        [HttpGet]
        public async Task<IActionResult> Index()
		{
			var data = await _context.Products
				.AsNoTracking()
				.Select(p => p.ToIndexVm())
				.ToListAsync();
			return View(data);
		}

		// 新增產品
		// GET: Products/Create
		[HttpGet]
		public async Task<IActionResult> CreateAsync() 
		{
            var model = new CreateProductViewModel();
            await LoadCategoryOptions(model);
            await LoadShipOptions(model); // 載入物流選項
			return View(model);
        }

		// 新增產品
		// POST: Products/Create
		[HttpPost]
		public async Task<IActionResult> Create(CreateProductViewModel vm)
		{
			// 1.基本驗證
			if (!ModelState.IsValid)
			{
				await LoadCategoryOptions(vm);
				await LoadShipOptions(vm); // 載入物流選項
				return View(vm);
			}

			// 2.類別驗證

			// 2.1 檢查是否有選擇類別
			if (vm.SelectedCategoryIds == null || !vm.SelectedCategoryIds.Any())
			{
				ModelState.AddModelError("SelectedCategoryIds", "請至少選擇一個商品類別");
			}
			else
			{
				// 2.2 檢查選中類別的有效性 
				// 檢查選中的類別ID是否都存在於資料庫中
				var allCategories = await _context.Categories
					.Where(c => c.IsActive)
					.ToListAsync();

				foreach (var categoryId in vm.SelectedCategoryIds)
				{
					// 檢查是否有選中的類別ID在資料庫中存在，有的話就取得該類別
					var category = allCategories.FirstOrDefault(c => c.Id == categoryId);

					// 如果類別不存在，則添加錯誤訊息
					if (category == null)
					{
						ModelState.AddModelError("SelectedCategoryIds", "選擇的類別不存在");
						break;
					}

					// 檢查是否有子類別（有子類別的不能選）
					bool hasChildren = allCategories.Any(c => c.FatherId == categoryId);
					if (hasChildren)
					{
						ModelState.AddModelError("SelectedCategoryIds", "不能選擇父類別");
						break;
					}

				}
			}

			// 3. 物流選項驗證
			// 3.1 檢查是否有選擇至少一個物流方式
			if (vm.SelectedShipIds == null || !vm.SelectedShipIds.Any())
			{

				ModelState.AddModelError("SelectedShipIds", "請至少選擇一個物流方式");
			}
			else
			{
				// 3.2 檢查選中物流方式的有效性
				var activeShips = await _context.Ships
				   .Where(s => s.IsActive == true)
				   .ToListAsync();

				foreach (var shipId in vm.SelectedShipIds)
				{
					var ship = activeShips.FirstOrDefault(s => s.Id == shipId);

					if (ship == null)
					{
						ModelState.AddModelError("SelectedShipIds", "選擇的物流方式不存在或已停用");
						break;
					}
				}
			}




			// 4.業務邏輯驗證

			// 4.1 檢查貨號是否重複

			// 檢查商品貨號是否已存在
			bool exists = _context.Products
						.Any(p => p.ItemNumber == vm.ItemNumber);
			// 如果已存在，則返回錯誤
			if (exists)
			{
				ModelState.AddModelError(nameof(vm.ItemNumber), "商品貨號已存在");
				return View(vm);
			}


			// 4.2 檢查價格是否為正數且大於0

			if (vm.Price <= 0)
			{
				ModelState.AddModelError(nameof(vm.Price), "價格必須大於0");
			}
			// 檢查是否為正整數
			if (vm.Price % 1 != 0)
			{
				ModelState.AddModelError(nameof(vm.Price), "台幣價格必須是整數");
			}

			// 4.3 檢查庫存為0時不能啟用
			if (vm.Quantity == 0 && vm.IsActive)
			{
				ModelState.AddModelError(nameof(vm.Quantity), "庫存為0時不能啟用商品");
			}

			// 5. 如果有任何驗證錯誤，則返回表單頁面

			if (!ModelState.IsValid)
			{
				// 重新載入類別選項
				await LoadCategoryOptions(vm);
				await LoadShipOptions(vm); // 載入物流選項
				return View(vm);
			}

			// 6. 驗證通過，開始建立資料
			try
			{
				// 建立商品實體
				var product = new Product
				{
					ItemNumber = vm.ItemNumber,
					Name = vm.Name,
					Keypoint = vm.Keypoint ?? "", // 確保不為 null
					ProductDescription = vm.ProductDescription ?? "", // 確保不為 null
					Price = vm.Price,
					Quantity = vm.Quantity,
					IsActive = vm.IsActive,
					CreateAt = DateTime.Now,
					UpdateAt = DateTime.Now
				};

				// 6.1 建立商品
				_context.Products.Add
					(product);
				await _context.SaveChangesAsync(); // 儲存商品以獲取 Id


				// 6.2 處理圖片
				var imageFileNames = new[]
				{
                // 宣告圖片檔案名稱，在 CreateProductViewModel 中已經定義了這些屬性
					vm.Image1FileName,
					vm.Image2FileName,
					vm.Image3FileName,
					vm.Image4FileName,
					vm.Image5FileName,
					vm.Image6FileName,
					vm.Image7FileName,
					vm.Image8FileName
				};
				// ✅ 批量查詢，避免多次資料庫呼叫
				var nonEmptyFileNames = imageFileNames
					.Where(f => !string.IsNullOrWhiteSpace(f))
					.ToList();
				// 一次性查詢所有需要的 FileId，避免重複追蹤

				if (nonEmptyFileNames.Any())
				{
					var fileIdMap = await _context.FileStreams
						.AsNoTracking()
						.Where(f => nonEmptyFileNames.Contains(f.FileName))
						.ToDictionaryAsync(f => f.FileName, f => f.Id);
					for (int i = 0; i < imageFileNames.Length; i++)
					{
						// 宣告圖片檔案名稱
						var fileName = imageFileNames[i];

						// 檢查圖片檔案名稱是否不為空
						if (!string.IsNullOrWhiteSpace(fileName) && fileIdMap.ContainsKey(fileName))

						{

							// 建立商品圖片關聯
							var productImage = new ProductImage
							{
								ProductId = product.Id,
								FileId = fileIdMap[fileName], // 使用預先查詢的 FileId
								SortOrder = i + 1 // 圖片順序從1開始
							};
							_context.ProductImages.Add(productImage);
							
						}
					}


				}

				
				// 6.3 建立商品類別關聯
				if (vm.SelectedCategoryIds?.Any() == true)
				{
					//categoryId 是選中的類別Id，從 CreateProductViewModel 中取得
					foreach (var categoryId in vm.SelectedCategoryIds)
					{
						var productCategory = new ProductCategory
						{
							ProductId = product.Id,
							CategoryId = categoryId
						};
						_context.ProductCategories.Add(productCategory);
					}
				}

				// 6.4 建立商品物流關聯
				if (vm.SelectedShipIds?.Any() == true)
				{
					// shipId 是選中的物流方式Id，從 CreateProductViewModel 中取得
					foreach (var shipId in vm.SelectedShipIds)
					{
						// 建立商品物流關聯
						// ProductShip 是一個中介表，用來連接 Product 和 Ship
						var productShip = new ProductShip
						{
							ProductId = product.Id,
							ShipId = shipId,
							IsActive = true // 預設為啟用狀態
						};
						_context.ProductShips.Add(productShip);
					}
				}

				// 7. 儲存變更  
				await _context.SaveChangesAsync();

				// 7. 成功後導向首頁
				// 8. 設定成功訊息並導向首頁 ← 新增這部分
				TempData["SuccessMessage"] = "商品新增成功！";
				return RedirectToAction("Index");


			}
			catch (Exception ex)
			{
				// 處理例外情況
				ModelState.AddModelError("", "建立商品時發生錯誤：" + ex.Message);
				await LoadCategoryOptions(vm);
				await LoadShipOptions(vm); // 載入物流選項
				return View(vm);
			}

		}


		// 編輯產品-載入編輯頁面
		// GET: Products/Edit/5
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			// 檢查商品是否存在，透過 GetProductByIdAsync 方法取得商品資料
			var product = await GetProductByIdAsync(id);
			if (product == null)
			{
				// 如果商品不存在，返回Index頁
				TempData["ErrorMessage"] = "商品不存在";
				return RedirectToAction(nameof(Index));
			}


			// 建立 ViewModel，取得資料庫中的商品資料
			var vm = new EditProductViewModel
			{
				Id = product.Id,
				ItemNumber = product.ItemNumber,
				Name = product.Name,
				Keypoint = product.Keypoint,
				ProductDescription = product.ProductDescription,
				Price = product.Price,
				Quantity = product.Quantity,
				IsActive = product.IsActive
			
			};

			// 載入現有圖片檔名
			var productImages = product.ProductImages
				.Where(pi=> pi.File != null && !string.IsNullOrEmpty(pi.File.FileName)) // 確保圖片檔案不為 null，且檔名不為空
				.OrderBy(pi => pi.SortOrder) // 按照圖片順序排序
				.ToList();
			// 將圖片檔名載入到 ViewModel 中
			// 用for 、switch 來載入圖片檔名
			for (int i=0; i < productImages.Count && i<8;i++)
			{
				var fileName = productImages[i].File.FileName;
				switch (i)
				{
					case 0: vm.Image1FileName = fileName; break;
					case 1: vm.Image2FileName = fileName; break;
					case 2: vm.Image3FileName = fileName; break;
					case 3: vm.Image4FileName = fileName; break;
					case 4: vm.Image5FileName = fileName; break;
					case 5: vm.Image6FileName = fileName; break;
					case 6: vm.Image7FileName = fileName; break;
					case 7: vm.Image8FileName = fileName; break;
				}
			
			}

			// 取的選中的商品類別
			vm.SelectedCategoryIds = product.ProductCategories
				.Select(pc => pc.CategoryId)
				.ToList();

			// 取得選中的物流方式
			vm.SelectedShipIds = product.ProductShips
				.Select(ps => ps.ShipId)
				.ToList();

			// 資料載入編輯模型中
			await LoadCategoryOptions(vm); // 載入類別選項
			await LoadShipOptions(vm); // 載入物流選項

			return View(vm);


		}


		// 編輯產品-送出編輯表單
		// POST: Products/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public  async Task<IActionResult> Edit(int id, EditProductViewModel vm)
		{
			// 1.檢查Id是否匹配
			if(id != vm.Id)
			{
				TempData["ErrorMessage"] = "找不到選取商品";
				return RedirectToAction(nameof(Index));
			}

			//檢查商品是否存在
			var existingProduct = await _context.Products
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.Id == id);
			if (existingProduct == null)
			{
				TempData["ErrorMessage"] = "商品不存在";
				return RedirectToAction(nameof(Index));
			}


			// 基本驗證
			// 檢查模型狀態是否有效，如果模型狀態無效，則返回表單頁面
			if (!ModelState.IsValid)
			{
				await LoadCategoryOptions(vm);
				await LoadShipOptions(vm);
				return View(vm);
			}

			// 2. 類別驗證 (表單欄位)
			// 類別驗證(像是新增，更新要確定類別)
			if (vm.SelectedCategoryIds == null || !vm.SelectedCategoryIds.Any())
			{
				ModelState.AddModelError("SelectedCategoryIds", "請至少選擇一個商品類別");
			}
			else
			{
				// 2.2 檢查選中類別的有效性 
				// 檢查選中的類別ID是否都存在於資料庫中
				var allCategories = await _context.Categories
					.Where(c => c.IsActive)
					.ToListAsync();

				foreach (var categoryId in vm.SelectedCategoryIds)
				{
					// 檢查是否有選中的類別ID在資料庫中存在，有的話就取得該類別
					var category = allCategories.FirstOrDefault(c => c.Id == categoryId);

					// 如果類別不存在，則添加錯誤訊息
					if (category == null)
					{
						ModelState.AddModelError("SelectedCategoryIds", "選擇的類別不存在");
						break;
					}

					// 檢查是否有子類別（有子類別的不能選）
					bool hasChildren = allCategories.Any(c => c.FatherId == categoryId);
					if (hasChildren)
					{
						ModelState.AddModelError("SelectedCategoryIds", "不能選擇父類別");
						break;
					}

				}
			}

			// 3. 物流選項驗證 (表單欄位)
			// 物流驗證(像是新增，更新要確定物流)
			if (vm.SelectedShipIds == null || !vm.SelectedShipIds.Any())
			{

				ModelState.AddModelError("SelectedShipIds", "請至少選擇一個物流方式");
			}
			else
			{
				// 3.2 檢查選中物流方式的有效性
				var activeShips = await _context.Ships
				   .Where(s => s.IsActive == true)
				   .ToListAsync();

				foreach (var shipId in vm.SelectedShipIds)
				{
					var ship = activeShips.FirstOrDefault(s => s.Id == shipId);

					if (ship == null)
					{
						ModelState.AddModelError("SelectedShipIds", "選擇的物流方式不存在或已停用");
						break;
					}
				}
			}

			//4. 業務邏輯驗證

			// 4.1 檢查貨號是否重複，且排除現在編輯的商品

			// 檢查商品貨號是否已存在
			bool itemNumberExists = await _context.Products
						.AnyAsync(p => p.ItemNumber == vm.ItemNumber && p.Id != vm.Id);
			// 如果已存在，則返回錯誤
			if (itemNumberExists)
			{
				ModelState.AddModelError(nameof(vm.ItemNumber), "商品貨號已存在");
			}


			// 4.2 檢查價格是否為正數且大於0

			if (vm.Price <= 0)
			{
				ModelState.AddModelError(nameof(vm.Price), "價格必須大於0");
			}
			// 檢查是否為正整數
			if (vm.Price % 1 != 0)
			{
				ModelState.AddModelError(nameof(vm.Price), "價格必須是整數");
			}



			// 4.3 檢查庫存為0時不能啟用
			if (vm.Quantity == 0 && vm.IsActive)
			{
				ModelState.AddModelError(nameof(vm.Quantity), "庫存為0時不能啟用商品");
			}
			// 5. 如果有任何驗證錯誤，則返回表單頁面
			if (!ModelState.IsValid)
			{
				// 重新載入類別選項
				await LoadCategoryOptions(vm);
				await LoadShipOptions(vm); // 載入物流選項
				return View(vm);
			}


			// 6.寫驗證通過，開始更新資料
			// 寫try catch 來處理例外情況，並且進行更新
			try
			{
				// 查詢現有實體進行更新
				var product = await _context.Products
					.FirstOrDefaultAsync(p => p.Id == vm.Id);
				if (product == null)
				{
					TempData["ErrorMessage"] = "商品不存在";
					return RedirectToAction(nameof(Index));
				}

				// 更新屬性
				product.ItemNumber = vm.ItemNumber;
				product.Name = vm.Name;
				product.Keypoint = vm.Keypoint ?? "";
				product.ProductDescription = vm.ProductDescription ?? "";
				product.Price = vm.Price;
				product.Quantity = vm.Quantity;
				product.IsActive = vm.IsActive;
				product.UpdateAt = DateTime.Now;

				// 更新product表單
				_context.Products.Update(product);

				// 處理圖片更新  (採用圖片更新方法)
				await UpdateProductImages(product.Id, vm);

				// 處理商品類別關聯更新 (採用類別更新方法)
				await UpdateProductCategories(product.Id, vm.SelectedCategoryIds);

				// 處理商品物流關聯更新 (採用物流更新方法)
				await UpdateProductShips(product.Id, vm.SelectedShipIds);

				// 儲存變更
				await _context.SaveChangesAsync();

				// 成功後導向首頁
				TempData["SuccessMessage"] = "商品編輯成功！";
				return RedirectToAction("Index");
			}
			catch (Exception ex) 
			{ 
				ModelState.AddModelError("", "更新商品時發生錯誤：" + ex.Message);
				// 如果發生錯誤，重新載入類別選項和物流選項
				await LoadCategoryOptions(vm);
				await LoadShipOptions(vm);
				
				return View(vm); // 返回編輯頁面
			}
		}

		private async Task UpdateProductShips(int productId, List<int>? selectedShipIds)
		{
			// 1.刪除現有的商品類別關聯
			// 1-1.先抓取現有的商品物流關聯
			var existingShips = await _context.ProductShips
				.Where(ps => ps.ProductId == productId)
				.ToListAsync();

			// 1-2.再刪除現有的商品物流關聯
			_context.ProductShips.RemoveRange(existingShips);

			// 2.建立新的商品物流關聯
			if (selectedShipIds?.Any() == true)
			{
				foreach (var shipId in selectedShipIds)
				{
					var productShip = new ProductShip
					{
						ProductId = productId,
						ShipId = shipId,
						IsActive = true // 預設為啟用狀態
					};
					_context.ProductShips.Add(productShip);
				}
			}
		}

		// 更新商品類別關聯的方法
		private async Task UpdateProductCategories(int productId, List<int>? selectedCategoryIds)
		{
			// 1.刪除現有的商品類別關聯
			// 1-1.先抓取現有的商品圖片關聯
			var existingCategories = await _context.ProductCategories
				.Where(pc => pc.ProductId == productId)
				.ToListAsync();

			// 1-2.再刪除現有的商品圖片關聯
			_context.ProductCategories.RemoveRange(existingCategories);

			// 2.新增類別關聯
			if (selectedCategoryIds?.Any() == true)
			{
				foreach (var categoryId in selectedCategoryIds)
				{
					var productCategory = new ProductCategory
					{
						ProductId = productId,
						CategoryId = categoryId
					};
					_context.ProductCategories.Add(productCategory);
				}
			}
		}

		private async Task UpdateProductImages(int id, EditProductViewModel vm)
		{
			// 刪除現有的商品圖片關聯
			// 先抓取現有的商品圖片關聯
			var existingImages = await _context.ProductImages
				.Where(pi => pi.ProductId == id)
				.ToListAsync();
				
			_context.ProductImages.RemoveRange(existingImages); // 刪除現有的商品圖片關聯

			// 新增圖片關連
			var imageFileNames = new[]
			{
				vm.Image1FileName,
				vm.Image2FileName,
				vm.Image3FileName,
				vm.Image4FileName,
				vm.Image5FileName,
				vm.Image6FileName,
				vm.Image7FileName,
				vm.Image8FileName
			};

			// 一次性查詢所有需要的 FileId，避免重複追蹤
			var nonEmptyFileNames = imageFileNames
				.Where(f => !string.IsNullOrWhiteSpace(f))
				.ToList();
			if (nonEmptyFileNames.Any())
			{
				var fileIdMap = await _context.FileStreams
				.AsNoTracking()
				.Where(f => nonEmptyFileNames.Contains(f.FileName)) // 只查詢非空的檔案名稱
				.ToDictionaryAsync(f => f.FileName, f => f.Id); // 建立檔案名稱到 FileId 的映射



				// 迭代圖片檔案名稱，建立新的商品圖片關聯
				for (int i = 0; i < imageFileNames.Length; i++)
				{
					var fileName = imageFileNames[i];
					if (!string.IsNullOrWhiteSpace(fileName))
					{
						// 建立商品圖片關聯
						var productImage = new ProductImage
						{
							ProductId = id,
							FileId = fileIdMap[fileName],//使用預先查詢的 FileId
							SortOrder = i + 1 // 圖片順序從1開始
						};
						_context.ProductImages.Add(productImage);
					}
				}
			}

		}
			
		



		// 根據 ID 取得商品完整資料(GetProductByIdAsync)
		private async Task<Product> GetProductByIdAsync(int id)
		{
			// 使用 AsNoTracking() 來避免 EF Core 的追蹤功能，這樣可以提高查詢性能
			return await _context.Products
				.AsNoTracking()
				.Include(p => p.ProductImages) // 包含商品圖片
					.ThenInclude(pi=> pi.File) // 包含圖片檔案資訊
				.Include(p => p.ProductCategories) // 包含商品類別
				.Include(p => p.ProductShips) // 包含商品物流
				.FirstOrDefaultAsync(p => p.Id == id);
		}




		// 載入物流選項
		// 這邊會載入所有啟用的物流方式，並將其加入到 CreateProductViewModel 的 Ships 屬性中
		private async Task LoadShipOptions(IProductViewModel vm)
		{
			// 取得啟用的物流選項
            var ships = await _context.Ships
                .Where(s => s.IsActive == true) // 只選取啟用的物流方式
				.OrderBy(s => s.ShipMethodName)
                .ToListAsync();

            // 判斷每個物流選項是否可選擇
            foreach (var ship in ships) { 
                
				// 將物流選項加入到模型的 Ships 屬性中
                vm.Ships.Add(new ShipSelectionItemViewModel
                {
                    Id = ship.Id,
                    ShipMethodName = ship.ShipMethodName,
                    ShipCost = ship.ShipCost,
                    IsActive = ship.IsActive ?? true  
                });


			}

		}

		// 載入類別選項
		private async Task LoadCategoryOptions(IProductViewModel model)
        {
            // 1. 取得所有啟用的類別
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // 2. 判斷每個類別是否可選擇
            foreach (var category in categories)
            {
                // 檢查是否有子類別
                // 這邊使用 Any() 方法來判斷是否有子類別，如果存在子類別，則 hasChildren 為 true
                bool hasChildren = categories.Any(c => c.FatherId == category.Id);

                // 判斷是否可選擇：沒有子類別的都可以選
                bool isSelectable = !hasChildren;

                // 將類別加入到模型的 Categories 屬性中
                model.Categories.Add(new CategorySelectionItemViewModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    FatherId = category.FatherId,
                    IsSelectable = isSelectable, // 是否可以選擇
                    Level = category.FatherId == null ? 0 : 1
                });
            }

        }





		/* 改批量查詢用不到
		 
		 // 從FileName 取得圖片Id
        public async Task<int?> GetFileIdByFileNameAsync(string fileName)
		{

            // 如果 FileName 為空 則返回 null

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }
            // 從 FileStreams 中查找對應的 FileId，這邊直接取的FileId

            return await _context.FileStreams
				.AsNoTracking() // 使用 AsNoTracking() 來避免 EF Core 的追蹤功能，這樣可以提高查詢性能
				.Where(f => f.FileName == fileName)
                .Select(f => f.Id) //選擇需要的欄位（SELECT 子句）
                .FirstOrDefaultAsync();
        }
		*/

	}
}
