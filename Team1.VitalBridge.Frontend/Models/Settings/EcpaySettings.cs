namespace Team1.VitalBridge.Frontend.Models.Settings
{
    public class EcpaySettings
    {
        public string MerchantId { get; set; } = string.Empty;
        public string HashKey { get; set; } = string.Empty;
        public string HashIV { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string NotifyUrl { get; set; } = string.Empty;

    }
}
