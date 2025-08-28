using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs.ECShop;
using Team1.VitalBridge.Frontend.Models.EFModels;
using Team1.VitalBridge.Frontend.Models.Settings;
using static Team1.VitalBridge.Frontend.Models.DTOs.ECShop.CheckoutDtos;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOptions<EcpaySettings> _ecpaySettings;
        private readonly IPaymentMethodMappingService _paymentMappingService;

        public OrdersController(AppDbContext context, IOptions<EcpaySettings> ecpaySettings, IPaymentMethodMappingService paymentMappingService)
        {
            this._context=context;
            _ecpaySettings = ecpaySettings;
            this._paymentMappingService=paymentMappingService;
        }

        /// <summary>
        /// 建立訂單並產生綠界付款表單
        /// 重點流程：
        /// 1. 驗證使用者和資料
        /// 2. 檢查庫存
        /// 3. 開始資料庫交易
        /// 4. 建立訂單相關記錄
        /// 5. 產生綠界付款表單
        /// </summary>
        /// 
     
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. 取得當前使用者

                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized("請重新登入");


                // 2. 驗證基本資料
                if (string.IsNullOrWhiteSpace(request.CustomerName) ||
                    string.IsNullOrWhiteSpace(request.CustomerEmail) ||
                    string.IsNullOrWhiteSpace(request.RecipientName))
                {
                    return BadRequest("缺少必要的訂單資料");
                }
                //var cart = await _context.Carts
                //    .Include(c => c.CartItems)
                //    .ThenInclude(ci => ci.Product)
                //    .FirstOrDefaultAsync(c => c.CustomerId == userId);
                // 3. 取得購物車資料並檢查庫存
                var cartItems = await _context.CartItems
                    .Where(ci => ci.Cart.CustomerId == userId)
                    .Include(ci => ci.Product)
                    .Where(ci => ci.Product.IsActive)
                    .ToListAsync();
                //if (!cartItems.Any())
                //    return BadRequest("購物車是空的");
                // 檢查庫存
                foreach (var item in cartItems)
                {
                    if (item.Product.Quantity < item.Quantity)
                    {
                        return BadRequest($"商品「{item.Product.Name}」庫存不足。可用庫存：{item.Product.Quantity}，需求數量：{item.Quantity}");
                    }
                }


                // 4. 驗證物流方式
                var ship = await _context.Ships.FirstOrDefaultAsync(s => s.Id == request.ShipId && s.IsActive == true); // 確認啟用
                if (ship == null)
                    return BadRequest("選擇的物流方式無效");

                // 5. 驗證付款方式
                var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.Id == request.PaymentMethodId && pm.IsActive);
                if (paymentMethod == null)
                    return BadRequest("選擇的付款方式無效");


                // 6. 產生訂單編號
                var orderNumber = await GenerateOrderNumber();

                // 7. 計算金額
                var subtotalAmount = cartItems.Sum(ci => ci.Quantity * ci.Product.Price);
                var shippingFee = ship.ShipCost;
                var totalAmount = subtotalAmount + shippingFee;

                // 8. 建立訂單主記錄
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    CustomerId = userId,
                    ShippingFee = shippingFee,
                    CouponId = null, // 目前不支援優惠券
                    CouponDiscount = null,
                    SubtotalAmount = subtotalAmount,
                    TotalAmount = totalAmount,
                    Note = request.Note ?? "",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now


                };
                _context.Orders.Add(order); // 這裡還沒 SaveChanges
                await _context.SaveChangesAsync(); // 保存取得 OrderId

                // 9.建立訂單明細
                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = cartItem.Product.Name,
                        UnitPrice = cartItem.Product.Price,
                        Quantity = cartItem.Quantity,
                        Subtotal = cartItem.Quantity * cartItem.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                    // 扣除庫存
                    cartItem.Product.Quantity -= cartItem.Quantity;

                }

                // 10. 建立收件人記錄
                var orderRecipient = new OrderRecipent
                {
                    OrderId = order.Id,
                    RecipentName = request.RecipientName,
                    RecipentPhone = request.RecipientPhone
                };
                _context.OrderRecipents.Add(orderRecipient);


                // 11. 建立物流記錄
                var orderShipMethod = new OrderShipMethod
                {
                    OrderId = order.Id,
                    ShipId = request.ShipId,
                    CityId = request.CityId,
                    TownshipId = request.TownshipId,
                    DetailAddress = request.DetailAddress,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.OrderShipMethods.Add(orderShipMethod);

                // 12. 建立訂單狀態記錄
                var orderStatus = new OrderStatus
                {
                    OrderId = order.Id,
                    OrderStatusItemId = 1, // 待付款
                    CreatedAt = DateTime.Now
                };
                _context.OrderStatuses.Add(orderStatus);

                // 13. 建立付款記錄
                var payment = new Payment
                {
                    OrderId = order.Id,
                    PayMethodId = request.PaymentMethodId,
                    Status = 1, // 待付款
                    Amount = totalAmount,
                    CreatAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Payments.Add(payment);

                // 14. 清空購物車
                _context.CartItems.RemoveRange(cartItems);

                // 15. 保存所有變更
                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();


                // 16.產生綠界付款表單資料
                var ecpayFormData = await GenerateEcpayFormData(order, request.CustomerName, request.CustomerEmail, request.PaymentMethodId);
                var response = new CreateOrderResponseDto
                {
                    Success = true,
                    Message = "訂單建立成功",
                    OrderNumber = orderNumber,
                    OrderId = order.Id,
                    EcpayFormData = ecpayFormData
                };

                return Ok(response);


            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                Console.WriteLine($"建立訂單錯誤: {ex.Message}");
                Console.WriteLine($"堆疊追蹤: {ex.StackTrace}");
                return StatusCode(500, "建立訂單失敗，請稍後再試");
            }


        }
        /// <summary>
        /// 產生綠界付款表單資料
        /// 這邊就會用到綠界設定的資料
        /// </summary>
        private async Task<EcpayFormDataDto> GenerateEcpayFormData(Order order, string customerName, string customerEmail, int paymentMethodId)
        {
            var ecpaySettings = _ecpaySettings.Value; // 取得設定

            // 使用服務來取得對應的綠界付款方式參數
            string choosePayment = _paymentMappingService.GetEcpayChoosePaymentById(paymentMethodId, _context);

           
            var formData = new Dictionary<string, string>
            {
                ["MerchantID"] = ecpaySettings.MerchantId,  // 改用設定
                ["MerchantTradeNo"] = order.OrderNumber,
                ["MerchantTradeDate"] = order.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss"),
                ["PaymentType"] = "aio",
                ["TotalAmount"] = ((int)order.TotalAmount).ToString(),
                ["TradeDesc"] = "VitalBridge商城購物",
                ["ItemName"] = GetOrderItemsDescription(order.Id),
                ["ReturnURL"] = ecpaySettings.ReturnUrl,     // 改用設定
                ["ClientBackURL"] = ecpaySettings.ClientBackUrl, // 改用設定
                ["OrderResultURL"] = ecpaySettings.OrderResultUrl, // 改用設定
                ["NeedExtraPaidInfo"] = "N",
                ["ChoosePayment"] = choosePayment, //動態設定付款方式
                ["PlatformID"] = "",
                ["InvoiceMark"] = "N",
                ["CustomField1"] = order.Id.ToString(),
                ["CustomField2"] = customerName,
                ["CustomField3"] = customerEmail,
                ["CustomField4"] = "",
                ["EncryptType"] = "1"
            };

			// 記錄 log 供除錯
			Console.WriteLine($"訂單 {order.OrderNumber} 綠界 URL 設定:");
			Console.WriteLine($"  ReturnURL (後端): {ecpaySettings.ReturnUrl}");
			Console.WriteLine($"  ClientBackURL (前端): {ecpaySettings.ClientBackUrl}");
			Console.WriteLine($"  OrderResultURL (前端): {ecpaySettings.OrderResultUrl}");

			// 產生檢查碼
			var checkMacValue = GenerateCheckMacValue(formData);
            formData.Add("CheckMacValue", checkMacValue);
            return new EcpayFormDataDto
            {
                FormAction = ecpaySettings.PaymentUrl, // 改用設定
                FormData = formData
            };

        }
		[HttpPost("ecpay-order-result")]
		[AllowAnonymous] // 允許綠界訪問
		public async Task<IActionResult> EcpayOrderResult([FromForm] EcpayReturnDto returnData)
		{
			try
			{
				// 接收綠界的 OrderResultURL POST 回傳
				var orderNumber = returnData.MerchantTradeNo;
				var rtnCode = returnData.RtnCode;

				// 不需要更新資料庫（ReturnURL 已經處理了）
				// 只負責重導向到前端頁面

				if (rtnCode == "1")
				{
					// 成功：重導向到付款成功頁面，並帶上訂單編號
					return Redirect($"https://localhost:7184/VitalBridge/ECshop/payment-success.html?orderNumber={orderNumber}");
				}
				else
				{
					// 失敗：重導向到付款失敗頁面
					return Redirect($"/ECshop/payment-failed.html?error={returnData.RtnMsg}");
				}
			}
			catch (Exception ex)
			{
				return Redirect("/ECshop/payment-failed.html?error=系統錯誤");
			}
		}



		/// <summary>
		/// 查詢訂單付款狀態 - 供前端付款結果頁面使用
		/// </summary>
		/// 

		[Authorize] // 需要登入
		[HttpGet("payment-status/{orderNumber}")]
        public async Task<IActionResult> GetOrderPaymentStatus(string orderNumber)
        {
			// 取得當前使用者 ID
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdStr, out var userId))
				return Unauthorized("請重新登入");

			try
            {
				var order = await _context.Orders
                    .Where(o => o.OrderNumber == orderNumber && o.CustomerId == userId) // 確保當前訂單屬於當前使用者
					.Select(o => new {
						o.OrderNumber,
						o.TotalAmount,
						o.CreatedAt,
						// 付款資訊
						PaymentStatusId = o.Payment != null ?   o.Payment.Status : (int?)null,
						PaymentStatusName = o.Payment != null
			                ? o.Payment.StatusNavigation.Name
			                : "尚未建立付款記錄",
						TransactionId = o.Payment != null ? o.Payment.TransactionId : null,
						PaidAt = o.Payment != null ? o.Payment.PaidAt : null,

						// 訂單狀態
						OrderStatusName = o.OrderStatuses
					        .OrderByDescending(os => os.CreatedAt)
					        .Select(os => os.OrderStatusItem.Name)
					        .FirstOrDefault() ?? "處理中"
					})
			        .FirstOrDefaultAsync();

				if (order == null)
					return NotFound(new { success = false, message = "訂單不存在" });
				// 回傳簡化的訂單狀態資訊
				var result = new
                {

					success = true,
					orderNumber = order.OrderNumber,
					totalAmount = order.TotalAmount,
					paymentStatus = order.PaymentStatusName,
					paymentStatusId = order.PaymentStatusId,
					transactionId = order.TransactionId,
					paidAt = order.PaidAt,
					orderStatus = order.OrderStatusName,
					createdAt = order.CreatedAt,
					isPaid = order.PaymentStatusId == 2
					// 2 = 已付款 (是資料庫的paymentStatus 裡面定義的id 2 代表 已付款)
				}; 
                return Ok(result);
			}
            catch (Exception ex) 
            {
				Console.WriteLine($"查詢訂單付款狀態錯誤: {ex.Message}");
				return StatusCode(500, new { success = false, message = "查詢失敗，請稍後再試" });
			}
        
        }

		/// <summary>
		/// 將資料庫的付款方式名稱對應到綠界的 ChoosePayment 參數
		/// </summary>
		private string GetEcpayChoosePayment(string paymentMethodName)
        {
            // 根據您資料庫中的付款方式名稱來對應綠界的參數
            return paymentMethodName?.ToLower() switch
            {
                "信用卡付款" => "Credit",           // 只顯示信用卡
                "信用卡" => "Credit",              // 只顯示信用卡
                "atm轉帳" => "ATM",               // 只顯示 ATM
                "atm付款" => "ATM",               // 只顯示 ATM  
                "網路atm" => "ATM",               // 只顯示 ATM
                "超商代碼繳費" => "CVS",            // 只顯示超商代碼
                "超商條碼繳費" => "BARCODE",        // 只顯示超商條碼
                "行動支付" => "AndroidPay",        // 行動支付相關
                _ => "ALL"                        // 預設顯示所有付款方式
            };
        }

        /// <summary>
        /// 產生綠界檢查碼
        /// </summary>
        private string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            var ecpaySettings = _ecpaySettings.Value; // 取得設定
            // 1. 排除CheckMacValue參數
            var sortedParams = parameters
                .Where(p => p.Key != "CheckMacValue")
                .OrderBy(p => p.Key)
                .ToList();
            // 2. 組合字串
            var rawString = string.Join("&", sortedParams.Select(p => $"{p.Key}={p.Value}"));
            
            // 3. 加入HashKey和HashIV（改用設定）
            var stringToHash = $"HashKey={ecpaySettings.HashKey}&{rawString}&HashIV={ecpaySettings.HashIV}";

            // 4. URL編碼
            stringToHash = HttpUtility.UrlEncode(stringToHash).ToLower();

            // 5. SHA256加密
            using (var sha256 = SHA256.Create())
            {
				// 這是綠界檢查碼資料https://developers.ecpay.com.tw/?p=2902
				var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        /// <summary>
        /// 取得訂單商品描述（綠界使用）
        /// </summary>
        private string GetOrderItemsDescription(int orderId)
        {
            var items = _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => $"{oi.ProductName}x{oi.Quantity}")
                .ToList();
            var description = string.Join("#", items);
            return description.Length > 200 ? description.Substring(0, 200) : description;
        }

        /// <summary>
        /// 產生訂單編號
        /// 格式: SO + 西元年月日時分 + 5位流水號
        /// </summary>
        private async Task<string> GenerateOrderNumber()
        {
            var now = DateTime.Now;
            var prefix = $"SO{now:yyyyMMddHHmm}";

            // 查詢同一分鐘內的最大流水號
            var existingOrders = await _context.Orders
                .Where(o => o.OrderNumber.StartsWith(prefix))
                .Select(o => o.OrderNumber)
                .ToListAsync();

            int maxSequence = 0;  // 最大流水號
            foreach (var orderNum in existingOrders)
            {
                if (orderNum.Length >= prefix.Length + 5)
                {
                    var sequenceStr = orderNum.Substring(prefix.Length);
                    if (int.TryParse(sequenceStr, out var sequence))
                    {
                        maxSequence = Math.Max(maxSequence, sequence);
                    }
                }
            }
            var newSequence = maxSequence + 1;
            return $"{prefix}{newSequence:D5}";


        }

        /// <summary>
        /// 查詢使用者訂單
        /// </summary>
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUserOrders([FromQuery] int? statusId = null)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized();

                var query = _context.Orders
                    .Where(o => o.CustomerId == userId)
                    .Include(o => o.OrderStatuses.OrderByDescending(os => os.CreatedAt))
                    .ThenInclude(os => os.OrderStatusItem)
                    .AsQueryable();
                // 根據狀態篩選
                if (statusId.HasValue)
                {
                    query = query.Where(o => o.OrderStatuses.First().OrderStatusItemId == statusId.Value);
                }

                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.TotalAmount,
                        o.CreatedAt,
                        Status = o.OrderStatuses.First().OrderStatusItem.Name,
                        StatusId = o.OrderStatuses.First().OrderStatusItemId
                    })
                    .ToListAsync();

                return Ok(orders);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"查詢訂單錯誤: {ex.Message}");
                return StatusCode(500, "查詢訂單失敗");
            }



        }
    }
}
