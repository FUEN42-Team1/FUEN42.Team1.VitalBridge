using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Models.Services
{
    public class PaymentMethodMappingService : IPaymentMethodMappingService
    {
                
            /// <summary>
            /// 根據付款方式名稱對應到綠界的 ChoosePayment 參數
            /// </summary>
            public string GetEcpayChoosePayment(string paymentMethodName)
            {
                if (string.IsNullOrEmpty(paymentMethodName))
                    return "ALL";

                // 將名稱轉為小寫並去除空白，方便比對
                var normalizedName = paymentMethodName.ToLower().Replace(" ", "");

                return normalizedName switch
                {
                    // 信用卡相關
                    "信用卡付款" or "信用卡" or "creditcard" or "credit" => "Credit",

                    // ATM 相關
                    "atm轉帳" or "atm付款" or "網路atm" or "atm虛擬帳號" or "atm" => "ATM",

                    // 超商相關
                    "超商代碼繳費" or "超商代碼" or "cvs" => "CVS",
                    "超商條碼繳費" or "超商條碼" or "barcode" => "BARCODE",

                    // 行動支付相關
                    "行動支付" or "google支付" or "googlepay" => "GooglePay",
                    "apple支付" or "applepay" => "ApplePay",
                    "samsung支付" or "samsungpay" => "SamsungPay",

                    // 電子錢包相關
                    "線上支付" or "ecpay錢包" or "ecpaywallet" => "Credit", // 通常歸類到信用卡

                    // 其他支付方式
                    "貨到付款" or "cod" => "ALL", // 綠界沒有貨到付款，顯示所有選項讓用戶選擇

                    // 預設：顯示所有付款方式
                    _ => "ALL"
                };
            }

        /// <summary>
        /// 根據付款方式ID查詢並對應到綠界參數（同步版本，避免在已有交易中再開啟async）
        /// </summary>
        public string GetEcpayChoosePaymentById(int paymentMethodId, AppDbContext context)
            {
                try
                {
                    var paymentMethod = context.PaymentMethods
                        .FirstOrDefault(pm => pm.Id == paymentMethodId);

                    return GetEcpayChoosePayment(paymentMethod?.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"查詢付款方式錯誤: {ex.Message}");
                    return "ALL"; // 發生錯誤時回傳預設值
                }
            }
        

    }
}
