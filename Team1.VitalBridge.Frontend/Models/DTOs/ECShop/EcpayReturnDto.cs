namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
	public class EcpayReturnDto
	{
		/// <summary>
		/// 特店訂單編號（您建立訂單時提供給綠界的編號）
		/// </summary>
		public string? MerchantTradeNo { get; set; }

		/// <summary>
		/// 綠界的交易編號
		/// </summary>
		public string? TradeNo { get; set; }

		/// <summary>
		/// 交易金額
		/// </summary>
		public int TradeAmt { get; set; }

		/// <summary>
		/// 付款時間 (yyyy/MM/dd HH:mm:ss)
		/// </summary>
		public string? PaymentDate { get; set; }

		/// <summary>
		/// 付款方式
		/// </summary>
		public string? PaymentType { get; set; }

		/// <summary>
		/// 回傳狀態碼 (1=成功, 其他=失敗)
		/// </summary>
		public string? RtnCode { get; set; }

		/// <summary>
		/// 回傳訊息
		/// </summary>
		public string? RtnMsg { get; set; }

		/// <summary>
		/// 交易狀態 (1=付款成功)
		/// </summary>
		public string? TradeStatus { get; set; }

		/// <summary>
		/// 檢查碼
		/// </summary>
		public string? CheckMacValue { get; set; }



	}
}
