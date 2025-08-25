using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Team1.VitalBridge.Frontend.Models.EFModels;
using static Team1.VitalBridge.Frontend.Models.DTOs.ECShop.CheckoutDtos;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // 綠界設定，待補
        private readonly string _merchantId = "3002607";
        private readonly string _hashKey = "pwFHCqoQZGmho4w6"; // 綠界提供的測試用 HashKey，在檢查碼計算時會用到
        private readonly string _hashIV = "EkRm7iFT261dpevs";   // 綠界提供的測試用 HashIV，在檢查碼計算時會用到
        private readonly string _ecpayUrl = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";

        public OrdersController(AppDbContext context, IConfiguration configuration)
        {
            this._context=context;
            this._configuration=configuration;
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
            using var transaction = await _context.Database.BeginTransactionAsync();

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


                // 3. 取得購物車資料並檢查庫存
                var cartItems = await _context.CartItems
                    .Where(ci => ci.Cart.CustomerId == userId)
                    .Include(ci => ci.Product)
                    .Where(ci => ci.Product.IsActive)
                    .ToListAsync();
                if (!cartItems.Any())
                    return BadRequest("購物車是空的");
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
                await transaction.CommitAsync();


                // 16.產生綠界付款表單資料
                var ecpayFormData = GenerateEcpayFormData(order, request.CustomerName, request.CustomerEmail);
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
                await transaction.RollbackAsync();
                Console.WriteLine($"建立訂單錯誤: {ex.Message}");
                Console.WriteLine($"堆疊追蹤: {ex.StackTrace}");
                return StatusCode(500, "建立訂單失敗，請稍後再試");
            }


        }
        /// <summary>
        /// 產生綠界付款表單資料
        /// 這邊就會用到綠界設定的資料
        /// </summary>
        private EcpayFormDataDto GenerateEcpayFormData(Order order, string customerName, string customerEmail)
        {
            var formData = new Dictionary<string, string>
            {
                ["MerchantID"] = _merchantId,
                ["MerchantTradeNo"] = order.OrderNumber,
                ["MerchantTradeDate"] = order.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss"),
                ["PaymentType"] = "aio",
                ["TotalAmount"] = ((int)order.TotalAmount).ToString(),
                ["TradeDesc"] = "VitalBridge商城購物",
                ["ItemName"] = GetOrderItemsDescription(order.Id),
                ["ReturnURL"] = $"https://localhost:7104/api/ecpay/PaymentCallback",
                ["ClientBackURL"] = "https://localhost:7184/VitalBridge/ECshop/index.html",
                ["OrderResultURL"] = "https://localhost:7184/VitalBridge/ECshop/index.html",
                ["NeedExtraPaidInfo"] = "N",
                ["ChoosePayment"] = "ALL",
                ["PlatformID"] = "",
                ["InvoiceMark"] = "N",
                ["CustomField1"] = order.Id.ToString(),
                ["CustomField2"] = customerName,
                ["CustomField3"] = customerEmail,
                ["CustomField4"] = "",
                ["EncryptType"] = "1"
            };
            // 產生檢查碼
            var checkMacValue = GenerateCheckMacValue(formData);
            formData.Add("CheckMacValue", checkMacValue);
            return new EcpayFormDataDto
            {
                FormAction = _ecpayUrl,
                FormData = formData
            };

        }

        /// <summary>
        /// 產生綠界檢查碼
        /// </summary>
        private string GenerateCheckMacValue(Dictionary<string, string> parameters)
        {
            // 1. 排除CheckMacValue參數
            var sortedParams = parameters
                .Where(p => p.Key != "CheckMacValue")
                .OrderBy(p => p.Key)
                .ToList();
            // 2. 組合字串
            var rawString = string.Join("&", sortedParams.Select(p => $"{p.Key}={p.Value}"));
            // 3. 加入HashKey和HashIV
            var stringToHash = $"HashKey={_hashKey}&{rawString}&HashIV={_hashIV}";

            // 4. URL編碼
            stringToHash = HttpUtility.UrlEncode(stringToHash).ToLower();

            // 5. SHA256加密
            using (var sha256 = SHA256.Create())
            {
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
