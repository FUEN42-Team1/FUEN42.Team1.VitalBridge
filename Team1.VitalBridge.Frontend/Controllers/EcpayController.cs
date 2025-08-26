using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Team1.VitalBridge.Frontend.Models.EFModels;
using Team1.VitalBridge.Frontend.Models.Settings;


namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EcpayController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOptions<EcpaySettings> _ecpaySettings;

        // 綠界設定（與 OrdersController 相同）
        //private readonly string _merchantId = "3002607";
        //private readonly string _hashKey = "pwFHCqoQZGmho4w6";
        //private readonly string _hashIV = "EkRm7iFT261dpevs";
        public EcpayController(AppDbContext context, IOptions<EcpaySettings> ecpaySettings)
        {
            
            this._context=context;
            this._ecpaySettings=ecpaySettings;
        }

        // <summary>
        /// 綠界付款結果通知回調
        /// 重點處理：
        /// 1. 驗證綠界回傳資料的完整性
        /// 2. 更新訂單付款狀態
        /// 3. 更新訂單處理狀態
        /// 4. 記錄交易資訊
        /// </summary>
        /// 

        // POST: api/Ecpay/PaymentCallback
        [HttpPost("PaymentCallback")]
        public async Task<IActionResult> PaymentCallback() 
        {

            try 
            {
                // 1. 讀取 POST 資料
                var formData = new Dictionary<string, string>();

                using (var reader = new StreamReader(Request.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    var pairs = body.Split('&');

                    foreach (var pair in pairs)
                    {
                        var keyValue = pair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            var key = HttpUtility.UrlDecode(keyValue[0]);
                            var value = HttpUtility.UrlDecode(keyValue[1]);
                            formData[key] = value;
                        }
                    }
                }

                // 記錄回傳資料供除錯
                Console.WriteLine($"綠界回傳資料: {string.Join(", ", formData.Select(kv => $"{kv.Key}={kv.Value}"))}");

                // 2. 驗證必要參數
                if (!formData.ContainsKey("MerchantTradeNo") ||
                    !formData.ContainsKey("RtnCode") ||
                    !formData.ContainsKey("CheckMacValue"))
                {
                    Console.WriteLine("綠界回傳資料缺少必要參數");
                    return BadRequest("缺少必要參數");
                }

                var orderNumber = formData["MerchantTradeNo"];
                var returnCode = formData["RtnCode"];
                var receivedCheckMacValue = formData["CheckMacValue"];

                // 3. 驗證檢查碼
                var calculatedCheckMacValue = GenerateCheckMacValue(formData);
                if (receivedCheckMacValue != calculatedCheckMacValue)
                {
                    Console.WriteLine($"檢查碼驗證失敗! 接收到: {receivedCheckMacValue}, 計算得: {calculatedCheckMacValue}");
                    return BadRequest("檢查碼驗證失敗");
                }
                // 4. 查詢訂單
                var order = await _context.Orders
                    .Include(o => o.Payment)
                    .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);

                if (order == null)
                {
                    Console.WriteLine($"找不到訂單: {orderNumber}");
                    return NotFound("訂單不存在");
                }

                // 5. 更新付款狀態
                //using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 更新 Payment 記錄
                    if (order.Payment != null)
                    {
                        if (returnCode == "1") // 付款成功
                        {
                            order.Payment.Status = 2; // 已付款
                            order.Payment.PaidAt = DateTime.Now;

                            // 記錄綠界交易編號
                            if (formData.ContainsKey("TradeNo"))
                            {
                                order.Payment.TransactionId = formData["TradeNo"];
                            }
                        }
                        else // 付款失敗
                        {
                            order.Payment.Status = 3; // 付款失敗
                        }

                        order.Payment.UpdatedAt = DateTime.Now;
                    }
                    // 新增訂單狀態記錄
                    var newOrderStatus = new OrderStatus
                    {
                        OrderId = order.Id,
                        OrderStatusItemId = returnCode == "1" ? 2 : 7, // 2:待出貨, 7:已取消
                        CreatedAt = DateTime.Now
                    };
                    _context.OrderStatuses.Add(newOrderStatus);

                    // 如果付款失敗，需要還原庫存
                    if (returnCode != "1")
                    {
                        var orderItems = await _context.OrderItems
                            .Where(oi => oi.OrderId == order.Id)
                            .Include(oi => oi.Product)
                            .ToListAsync();

                        foreach (var orderItem in orderItems)
                        {
                            orderItem.Product.Quantity += orderItem.Quantity;
                        }
                    }
                    await _context.SaveChangesAsync();
                    //await transaction.CommitAsync();

                    Console.WriteLine($"訂單 {orderNumber} 付款狀態更新完成，結果: {(returnCode == "1" ? "成功" : "失敗")}");

                    // 6. 回傳給綠界的確認訊息
                    return Ok("1|OK");
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    Console.WriteLine($"更新訂單狀態錯誤: {ex.Message}");
                    return StatusCode(500, "更新訂單狀態失敗");
                }


            }
            catch (Exception ex)             
            {
                Console.WriteLine($"處理綠界回調錯誤: {ex.Message}");
                Console.WriteLine($"堆疊追蹤: {ex.StackTrace}");
                return StatusCode(500, "處理付款回調失敗");
            }
            
        }

        /// <summary>
        /// 產生綠界檢查碼（與 OrdersController 相同邏輯）
        /// </summary>
        /// 
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
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(stringToHash));
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }

        }

        /// <summary>
        /// 測試用：查詢付款狀態
        /// </summary>
        /// 
        [HttpGet("PaymentStatus/{orderNumber}")]
        public async Task<IActionResult> GetPaymentStatus(string orderNumber)
        {
            try 
            {
                var order = await _context.Orders
                   .Include(o => o.Payment)
                   .ThenInclude(p => p.StatusNavigation)
                   .Include(o => o.OrderStatuses.OrderByDescending(os => os.CreatedAt))
                   .ThenInclude(os => os.OrderStatusItem)
                   .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
                if (order == null)
                    return NotFound("訂單不存在");

                var result = new
                {
                    OrderNumber = order.OrderNumber,
                    TotalAmount = order.TotalAmount,
                    PaymentStatus = order.Payment?.StatusNavigation?.Name ?? "無付款記錄",
                    PaymentStatusId = order.Payment?.Status,
                    TransactionId = order.Payment?.TransactionId,
                    PaidAt = order.Payment?.PaidAt,
                    OrderStatus = order.OrderStatuses.FirstOrDefault()?.OrderStatusItem?.Name ?? "無狀態",
                    OrderStatusId = order.OrderStatuses.FirstOrDefault()?.OrderStatusItemId,
                    CreatedAt = order.CreatedAt
                };

                return Ok(result);



            }
            catch (Exception ex)
            {
                Console.WriteLine($"查詢付款狀態錯誤: {ex.Message}");
                return StatusCode(500, "查詢付款狀態失敗");
            }
        
        }


    }
}
