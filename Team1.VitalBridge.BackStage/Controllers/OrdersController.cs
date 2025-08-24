using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
	public class OrdersController : Controller
	{
		private readonly AppDbContext _context;

		public OrdersController(AppDbContext context)
		{
			this._context = context;
		}

		public async  Task<IActionResult> Index(int? status, string? keyword , int page = 1, int pageSize = 10)
		{

			// 基本查詢

			var query = _context.Orders
				.Include(o => o.Customer) // 包含顧客資訊，就是User模型
				.Include(o => o.OrderStatuses) // 包含訂單狀態資訊
					.ThenInclude(os => os.OrderStatusItem)
				.Include(o => o.OrderShipMethod) // 包含訂單運送方式資訊
					.ThenInclude(osm => osm.Ship) // 包含運送方式詳細資訊
				.Include(o =>o.Payment) // 包含訂單付款資訊
					.ThenInclude(p => p.StatusNavigation)
				.AsQueryable();


			//搜尋關鍵字篩選

			if(!string.IsNullOrEmpty(keyword)) // 搜尋關鍵字參數帶入
			{
				query = query.Where(o =>
					o.OrderNumber.Contains(keyword) ||
					o.Customer.Name.Contains(keyword));
			}

			//狀態篩選
			// if裡面的status參數是訂單狀態的Id
			if (status.HasValue)
			{
				query = query.Where(o =>
				o.OrderStatuses // 包含訂單狀態資訊
					.OrderByDescending(os => os.CreatedAt)
					.First()
					.OrderStatusItemId == status.Value);

			}


			// 計算總筆數
			var totalCount = await query.CountAsync();


			// 分頁查詢 會用page和pageSize參數
			var orders = await query
				.OrderByDescending(o => o.CreatedAt) // 按照建立時間降序排列
				.Skip((page - 1) * pageSize) // 跳過前面的頁數
				.Take(pageSize) // 取得當前頁的資料
				.Select(o => new OrderListItemViewModel
				{
					Id= o.Id,
					OrderNumber = o.OrderNumber,
					CustomerName = o.Customer.Name,
					CreatedAt = o.CreatedAt,
					PaymentStatus = o.Payment != null && o.Payment.StatusNavigation != null
						? o.Payment.StatusNavigation.Name
						: "待付款",
					ShippingMethod = o.OrderShipMethod != null && o.OrderShipMethod.Ship != null
						? o.OrderShipMethod.Ship.ShipMethodName : "未設定",
					// 直接從資料庫取得狀態名稱
					CurrentStatusName = o.OrderStatuses.Any()
						? o.OrderStatuses.OrderByDescending(os => os.CreatedAt).First().OrderStatusItem.Name
						: "待付款", // 預設狀態名稱

					TotalAmount = o.TotalAmount // 訂單總金額
				})
				.ToListAsync();


			// 建立狀態選項(取得各狀態的訂單數量)
			var statusOptions = await GetStatusOptionsAsync(status);


			//組裝ViewModel
			var viewModel = new OrderListViewModel
			{
				Orders = orders,
				Pagination= new PaginationViewModel
				{
					CurrentPage = page,
					PageSize = pageSize,
					TotalCount = totalCount,
					
				},

				Search = new OrderSearchViewModel
				{
					
					Keyword = keyword,
					StatusFilter = status,
					Page = page,
					PageSize = pageSize

				},
				StatusOptions = statusOptions


			};

			return View(viewModel);
		}

		// 輔助方法：取得狀態選項和數量
		private async Task<List<OrderStatusOptionViewModel>> GetStatusOptionsAsync(int? currentStatus)
		{
			var statusOptions = new List<OrderStatusOptionViewModel>();

			// 全部訂單
			var TotalCout = await _context.Orders.CountAsync();
			statusOptions.Add(new OrderStatusOptionViewModel
			{
				StatusId = null, // null表示全部訂單
				StatusName = "全部訂單",
				Count = TotalCout,
				IsActive =  currentStatus == null // 判斷是否為當前選中狀態
			});


			// 按照順序：待付款、待出貨、已出貨、已完成、退貨中、已退貨、已取消
			//var statusOrder = new[] { 1, 2, 3, 4, 5, 6, 7 }; // 待付款=1, 待出貨=2, 已出貨=3, 已完成=4, 退貨中=5, 已退貨=6, 已取消=7

			// 從資料庫取得所有狀態項目
			var statusItems = await _context.OrderStatusItems
				.OrderBy(osi => osi.Id)
				.ToListAsync();

			foreach (var statusItem in statusItems)
			{
				var count = await _context.Orders
					.Where(o => o.OrderStatuses
						.OrderByDescending(os => os.CreatedAt)
						.FirstOrDefault().OrderStatusItemId == statusItem.Id)
					.CountAsync();

				statusOptions.Add(new OrderStatusOptionViewModel
				{
					StatusId = statusItem.Id,
					StatusName = statusItem.Name, // 直接使用資料庫的名稱
					Count = count,
					IsActive = currentStatus == statusItem.Id
				});
			}
			return statusOptions;
		}

		// 輔助方法：取得狀態名稱
		private string GetStatusName(int statusId)
		{
			
			return statusId switch
			{
				1 => "待付款",
				2 => "待出貨",
				3 => "已出貨",
				4 => "已完成",
				5 => "退貨中",
				6 => "已退貨",
				7 => "已取消",
				_ => "未知狀態"
			};
		}
	}
}
