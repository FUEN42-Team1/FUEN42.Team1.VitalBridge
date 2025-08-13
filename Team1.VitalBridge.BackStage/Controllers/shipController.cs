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
        public async Task<IActionResult> Index()
        {
			// 列出所有物流選項
			var shipsOption =  await _context.Ships
                .AsNoTracking()
                .Include(s => s.OrderShipMethods)
                .Select( s=> new shipViewModel
                {
                    Id = s.Id,
                    ShipMethodName = s.ShipMethodName,
                    ShipCost = (int)s.ShipCost,
                    IsActive = s.IsActive
				}
				)
                .ToListAsync();
			return View(shipsOption);
        }

        [HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Create(CreateShipViewModel vm)
		{
			// 驗證模型狀態
            if(ModelState.IsValid == false) return View(vm);

			// 檢查運送方式名稱是否已存在

			bool exists= _context.Ships
				.Any(s => s.ShipMethodName == vm.ShipMethodName);
			// 如果已存在，則返回錯誤
			if (exists)
			{
				ModelState.AddModelError(nameof(vm.ShipMethodName), "運送方式名稱已存在");
				return View(vm);
			}

			var ship = new Ship
			{
				ShipMethodName = vm.ShipMethodName,
				ShipCost = vm.ShipCost,
				IsActive = vm.IsActive
			};
			_context.Ships.Add(ship);
			_context.SaveChanges();

			// 重定向到索引頁面

			TempData["SuccessMessage"] = "新增運送方式成功";
			return RedirectToAction(nameof(Index));
		}

		// GET: ship/Edit/5
		[HttpGet]
		public async Task <IActionResult> Edit(int? id)
		{
			if(id==null)
			{
				return NotFound();
			}
			var ship = await _context.Ships.FindAsync(id);

			if(ship == null)
			{
				return NotFound();
			}

			// 將 Ship 實體轉換為 EditShipViewModel
			// 並返回視圖，給使用看的畫面
			// 這樣可以避免直接在視圖中使用 EF 實體，保持視圖的簡潔性和安全性
			
			var vm = new EditShipViewModel
			{
				Id = ship.Id,
				ShipMethodName = ship.ShipMethodName,
				ShipCost = ship.ShipCost,
				IsActive = (bool)ship.IsActive
			}; return View(vm);


		}

		// POST: ship/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, EditShipViewModel vm)
		{
			// 先驗證Id 是否相等
			if (id !=vm.Id)
			{
				return NotFound();
			}

			// 驗證模型狀態
			if (ModelState.IsValid)
			{
				
				try
				{
					// 把 EditShipViewModel 轉換為 Ship 實體
					_context.Ships.Update(new Ship
					{
						Id = vm.Id,
						ShipMethodName = vm.ShipMethodName,
						ShipCost = vm.ShipCost,
						IsActive = vm.IsActive
					});
					// 儲存變更到資料庫
					await _context.SaveChangesAsync();

				}
				catch (DbUpdateConcurrencyException)
				{
					// 顯示錯誤訊息或記錄錯誤
					ModelState.AddModelError("", "資料儲存時發生衝突，請稍後再試。");
					return View(vm);
				}
			
			
			}

			TempData["EditMessage"] = "編輯成功";
			return RedirectToAction(nameof(Index));
		}

		// 刪除運送方式
		// GET: ship/Delete/5

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var ship = await _context.Ships
				.Include(s => s.ProductShips) // 包含相關的 ProductShips
				.FirstOrDefaultAsync(s => s.Id == id);
			// 如果找不到該運送方式，則返回 NotFound
			if (ship == null)
			{
				return NotFound();
			}
			// 檢查是否有相關的 ProductShips
			if (ship.ProductShips.Any())
			{
				// 如果有相關的 ProductShips，則返回錯誤訊息
				TempData["DeleteError"] = "此物流選項已被商品或訂單使用，無法刪除。";
				return RedirectToAction(nameof(Index));
			}

			_context.Ships.Remove(ship);
			await _context.SaveChangesAsync();

			TempData["DeleteMessage"] = "刪除成功";
			return RedirectToAction(nameof(Index));
		}
	}
}
