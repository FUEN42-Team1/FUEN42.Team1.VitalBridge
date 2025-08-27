using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using static Team1.VitalBridge.BackStage.Models.ViewModels.OrderDetailsViewModel;
using Microsoft.EntityFrameworkCore.Storage; // 新增這個，for transaction

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

		public async Task<IActionResult> Details(int id) 
		{
			var order = await _context.Orders
				.Include(o=>o.Customer)
				.Include(o=>o.OrderRecipent)
				.Include(o => o.Payment)
					.ThenInclude(p=>p.PayMethod)
				.Include(o=>o.Payment)
					.ThenInclude(ss=>ss.StatusNavigation) // StatusNavigation 這是付款狀態(確認是 已付款、付款失敗)，PaymentStatus
                .Include(o=>o.OrderShipMethod)
					.ThenInclude(osm => osm.Ship)
                .Include(o => o.OrderShipMethod)
					.ThenInclude(osm => osm.City)
				.Include(o => o.OrderShipMethod)
					.ThenInclude(osm => osm.Township)
                 .Include(o => o.OrderItems)
					.ThenInclude(oi => oi.Product)
				.Include(o => o.OrderStatuses)
					.ThenInclude(os => os.OrderStatusItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var viewModel = MapToViewModel(order);
            return View(viewModel);
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateShippingStatus(int orderId, string shippingStatus, string trackingCode)
		{
			if (string.IsNullOrEmpty(shippingStatus) || string.IsNullOrEmpty(trackingCode))
			{
				TempData["Error"] = "請填寫完整的物流狀態和追蹤碼";
				return RedirectToAction("Details", new { id = orderId });
			}
			// 驗證追蹤碼格式（只能包含英文和數字）
			if (!Regex.IsMatch(trackingCode, @"^[A-Za-z0-9]+$"))
			{
				TempData["Error"] = "追蹤碼僅能包含英文字母和數字";
				return RedirectToAction("Details", new { id = orderId });
			}
			try
			{
				// 使用交易確保資料一致性
				// 注意這一行有可能因此報錯
				//using var transaction = await _context.Database.BeginTransactionAsync();

				// 取得訂單資訊
				var order = await _context.Orders
					.Include(o => o.OrderShipMethod)
						.ThenInclude(osm => osm.Ship)
					.Include(o => o.Payment)
						.ThenInclude(p => p.StatusNavigation)
					.Include(o => o.OrderStatuses)
						.ThenInclude(os => os.OrderStatusItem)
					.FirstOrDefaultAsync(o => o.Id == orderId);

				if (order == null)
				{
					TempData["Error"] = "找不到指定的訂單";
					return RedirectToAction("Details", new { id = orderId });
				}

				// 驗證業務規則
				var validationResult = ValidateShippingStatusUpdate(order, shippingStatus, trackingCode);
				if (!validationResult.IsValid)
				{
					TempData["Error"] = validationResult.ErrorMessage;
					return RedirectToAction("Details", new { id = orderId });
				}
				// 更新追蹤碼
				if (order.OrderShipMethod.Ship.ShipMethodName == "宅配")
				{
					order.OrderShipMethod.HomeTrackingCode = trackingCode;
				}
				else
				{
					order.OrderShipMethod.StoreTrackingCode = trackingCode;
				}
				order.OrderShipMethod.UpdatedAt = DateTime.Now;

				// 如果設定為已配送，自動新增訂單狀態記錄
				if (shippingStatus == "已配送")
				{
					var newOrderStatus = new OrderStatus
					{
						OrderId = orderId,
						OrderStatusItemId = 3, // 已出貨
						CreatedAt = DateTime.Now
					};
					_context.OrderStatuses.Add(newOrderStatus);
				}
				await _context.SaveChangesAsync();
				//await transaction.CommitAsync();

				TempData["Success"] = "物流狀態更新成功";

			}
			catch (Exception ex)
			{
				TempData["Error"] = "更新物流狀態時發生錯誤：" + ex.Message;

			}
			return RedirectToAction("Details", new { id = orderId });
		}

		// 申請退貨
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ApplyReturn(int orderId)
		{
			try 
			{
				// 先取得訂單資訊
				var order = await _context.Orders
					.Include(o => o.OrderStatuses)
						.ThenInclude(os => os.OrderStatusItem)
					.Include(o => o.OrderShipMethod)
					.FirstOrDefaultAsync(o => o.Id == orderId);

				if (order == null)
				{
					TempData["Error"] = "找不到指定的訂單";
					return RedirectToAction("Details", new { id = orderId });
				}

				// 驗證是否可以申請退貨
				var latestStatus = order.OrderStatuses.OrderByDescending(os => os.CreatedAt).FirstOrDefault();
				var hasTrackingCode = !string.IsNullOrEmpty(order.OrderShipMethod?.HomeTrackingCode) ||
									  !string.IsNullOrEmpty(order.OrderShipMethod?.StoreTrackingCode);
				if (latestStatus?.OrderStatusItemId != 3 || !hasTrackingCode) // 不是已出貨狀態或沒有追蹤碼
				{
					TempData["Error"] = "此訂單目前無法申請退貨";
					return RedirectToAction("Details", new { id = orderId });
				}



				// 新增退貨中狀態
				var returnStatus = new OrderStatus
				{
					OrderId = orderId,
					OrderStatusItemId = 5, // 退貨中
					CreatedAt = DateTime.Now
				};
				_context.OrderStatuses.Add(returnStatus);
				await _context.SaveChangesAsync();
				TempData["Success"] = "已成功申請退貨，訂單狀態已更新為退貨中";


			} catch (Exception ex) 
			{
				TempData["Error"] = "申請退貨時發生錯誤：" + ex.Message;
			}
			return RedirectToAction("Details", new { id = orderId });

		}

		// 更新訂單備註
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateNote(int orderId, string note)
		{
			try
			{
				// 取得訂單
				var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
				if (order == null)
				{
					TempData["Error"] = "找不到指定的訂單";
					return RedirectToAction("Details", new { id = orderId });
				}
				order.Note = note ?? "";
				order.UpdatedAt = DateTime.Now;

				await _context.SaveChangesAsync();

				TempData["Success"] = "訂單備註已更新";
			}
			catch (Exception ex)
			{
				TempData["Error"] = "更新訂單備註時發生錯誤：" + ex.Message;
			}
			return RedirectToAction("Details", new { id = orderId });
		}


		// 輔助方法：驗證物流狀態更新
		private ValidationResult ValidateShippingStatusUpdate(Order order, string shippingStatus, string trackingCode)
		{
			// 檢查付款狀態
			if (order.Payment?.Status != 2) // 必須已付款
			{
				return new ValidationResult(false, "訂單尚未付款，無法更新物流狀態");
			}

			// 檢查目前物流狀態
			var currentShippingStatus = GetShippingStatus(order.OrderShipMethod, order.OrderStatuses);

			if (currentShippingStatus == "已配送")
			{
				return new ValidationResult(false, "訂單已配送，無法再次更新物流狀態");
			}

			if (currentShippingStatus == "退貨中")
			{
				return new ValidationResult(false, "訂單處於退貨中狀態，無法更新物流狀態");
			}

			// 檢查是否從已配送改回未配送（不允許）
            if (shippingStatus == "未配送" && currentShippingStatus == "已配送")
			{
				return new ValidationResult(false, "無法將已配送狀態改回未配送");
			}

			return new ValidationResult(true, "");



		}


		// 輔助方法：將 Order 映射到 OrderDetailsViewModel
		private OrderDetailsViewModel MapToViewModel(Order order)
        {
            var latestOrderStatus = order.OrderStatuses
				 .OrderByDescending(os => os.CreatedAt)
				 .FirstOrDefault();
            var shippingStatus = GetShippingStatus(order.OrderShipMethod, order.OrderStatuses);
            
			// 判斷是否為宅配
			var isHomeDelivery = order.OrderShipMethod?.Ship?.ShipMethodName == "宅配";

            // 三元運算子取得對應的追蹤碼
            var trackingCode = isHomeDelivery
				? order.OrderShipMethod?.HomeTrackingCode
				: order.OrderShipMethod?.StoreTrackingCode;

            // 組合地址
            var shippingAddress = "";
			if (isHomeDelivery)
			{
				shippingAddress = $"{order.OrderShipMethod?.City?.Name ?? ""}" +
						 $"{order.OrderShipMethod?.Township?.Name ?? ""}" +
						 $"{order.OrderShipMethod?.DetailAddress ?? ""}";
			}
			else 
			{
                shippingAddress = $"{order.OrderShipMethod?.StoreCode ?? ""} - " +
                         $"{order.OrderShipMethod?.StoreAddress ?? ""}";
            }

			var viewModel = new OrderDetailsViewModel
			{
				// 訂單基本資訊
				OrderId = order.Id,
                OrderNumber= order.OrderNumber,
                CreatedAt= order.CreatedAt,
                Note= order.Note,
                TotalAmount= order.TotalAmount,

                // 會員資訊
                CustomerName = order.Customer?.Name,
				CustomerEmail= order.Customer?.Email,
				CustomerPhone= order.Customer?.Phone,

                // 收件人資訊
                RecipientName = order.OrderRecipent?.RecipentName??"",
				RecipientPhone = order.OrderRecipent?.RecipentPhone??"",


                // 付款資訊
                PaymentMethodName = order.Payment?.PayMethod?.Name ?? "",
                PaymentStatusName = order.Payment?.StatusNavigation?.Name ?? "待付款",
                PaymentAmount = order.Payment?.Amount ?? order.TotalAmount,
                PaymentStatusId = order.Payment?.Status ?? 1,


                // 物流資訊
                ShipId = order.OrderShipMethod?.ShipId ?? 0,
                ShippingMethodName = order.OrderShipMethod?.Ship?.ShipMethodName ?? "",
                ShippingAddress = shippingAddress,
                TrackingCode = trackingCode ?? "",
                ShippingStatus = shippingStatus,
                IsHomeDelivery = isHomeDelivery,


                // 商品明細
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    Subtotal = oi.Subtotal
                }).ToList(),


                // 價格明細
                SubtotalAmount = order.SubtotalAmount,
                ShippingFee = order.ShippingFee,
                CouponDiscount = order.CouponDiscount,


                // 狀態資訊
                CurrentOrderStatus = latestOrderStatus?.OrderStatusItem?.Name ?? "待付款",
                CurrentOrderStatusId = latestOrderStatus?.OrderStatusItemId ?? 1

            };
            // 設定編輯權限
            SetEditPermissions(viewModel);
            return viewModel;
        }

		// 輔助類別：驗證結果
		public class ValidationResult
		{
			public bool IsValid { get; set; }
			public string ErrorMessage { get; set; }

			public ValidationResult(bool isValid, string errorMessage)
			{
				IsValid = isValid;
				ErrorMessage = errorMessage;
			}
		}

		private string GetShippingStatus(OrderShipMethod shipMethod, ICollection<OrderStatus> orderStatuses)
        {
            // 物流狀態判斷和權限設定方法

			var latestStatus = orderStatuses
				.OrderByDescending(x=>x.CreatedAt)
				.FirstOrDefault();

            if (latestStatus?.OrderStatusItem.Name == "退貨中" || latestStatus?.OrderStatusItem.Name == "已退貨")
                return "退貨中";

			if(shipMethod?.Ship?.ShipMethodName == "宅配")
				return string.IsNullOrEmpty(shipMethod.HomeTrackingCode) ? "未配送" : "已配送";
            else
                return string.IsNullOrEmpty(shipMethod?.StoreTrackingCode) ? "未配送" : "已配送";
        }

		private void SetEditPermissions(OrderDetailsViewModel viewModel)
		{
            // 只有未配送且已付款才能編輯物流
			viewModel.CanEditShipping = viewModel.CurrentOrderStatusId == 2 &&

                                        viewModel.PaymentStatusId == 2; // 已付款=2

            // 只有已配送且已出貨狀態才能申請退貨
            viewModel.CanApplyReturn =viewModel.CurrentOrderStatusId == 3; // 已出貨

        }


        // 輔助方法：取得狀態名稱
        //private string GetStatusName(int statusId)
        //{

        //	return statusId switch
        //	{
        //		1 => "待付款",
        //		2 => "待出貨",
        //		3 => "已出貨",
        //		4 => "已完成",
        //		5 => "退貨中",
        //		6 => "已退貨",
        //		7 => "已取消",
        //		_ => "未知狀態"
        //	};
        //}




    }
}
