namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
	public class CheckoutDtos
	{
		/// <summary>
		/// 結帳初始化回應資料
		/// </summary>
		public class CheckoutInitResponseDto
		{
			public CustomerInfoDto CustomerInfo { get; set; }
			public List<CheckoutCartItemDto> CartItems { get; set; }
			public List<ShippingMethodDto> AvailableShippingMethods { get; set; }
			public List<PaymentMethodDto> PaymentMethods { get; set; }
			public List<CityDto> Cities { get; set; }
			public decimal Subtotal { get; set; }
		}

		/// <summary>
		/// 會員資料DTO
		/// </summary>
		public class CustomerInfoDto
		{
			public string Name { get; set; }
			public string Email { get; set; }
			public string Phone { get; set; }
		}

		/// <summary>
		/// 結帳用購物車項目DTO
		/// </summary>
		public class CheckoutCartItemDto
		{
			public int CartItemId { get; set; }
			public int ProductId { get; set; }
			public string ProductName { get; set; }
			public decimal UnitPrice { get; set; }
			public int Quantity { get; set; }
			public decimal Subtotal { get; set; }
			public int Stock { get; set; }
			public string ImageFileName { get; set; }
			public string ItemNumber { get; set; }
		}

		/// <summary>
		/// 物流方式DTO
		/// </summary>
		public class ShippingMethodDto
		{
			public int ShipId { get; set; }
			public string ShipMethodName { get; set; }
			public decimal ShipCost { get; set; }
		}

		/// <summary>
		/// 付款方式DTO
		/// </summary>
		public class PaymentMethodDto
		{
			// 這是 API 傳輸用的簡化版本，只包含 Id 和 Name，讓前端顯示選項
			public int Id { get; set; }
			public string Name { get; set; }
		}

		/// <summary>
		/// 縣市DTO
		/// </summary>
		public class CityDto
		{
			public int Id { get; set; }
			public string Name { get; set; }
		}

		/// <summary>
		/// 鄉鎮區DTO
		/// </summary>
		public class TownshipDto
		{
			public int Id { get; set; }
			public string Name { get; set; }
			public string PostalCode { get; set; }
		}
		/// <summary>
		/// 建立訂單請求DTO
		/// </summary>
		public class CreateOrderRequestDto
		{
			// 顧客資訊
			public string CustomerName { get; set; }
			public string CustomerEmail { get; set; }
			public string CustomerPhone { get; set; }

			// 收件人資訊
			public string RecipientName { get; set; }
			public string RecipientPhone { get; set; }
			public int CityId { get; set; }
			public int TownshipId { get; set; }
			public string DetailAddress { get; set; }

			// 物流與付款
			public int ShipId { get; set; }
			public int PaymentMethodId { get; set; }

			// 其他
			public string Note { get; set; }
		}

		/// <summary>
		/// 建立訂單回應DTO
		/// </summary>
		public class CreateOrderResponseDto
		{
			public bool Success { get; set; }
			public string Message { get; set; }
			public string OrderNumber { get; set; }
			public int OrderId { get; set; }

			// 綠界付款表單資料
			public EcpayFormDataDto EcpayFormData { get; set; }
		}

		/// <summary>
		/// 綠界付款表單資料DTO
		/// </summary>
		public class EcpayFormDataDto
		{
			public string FormAction { get; set; } = "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5";
			public Dictionary<string, string> FormData { get; set; } = new Dictionary<string, string>();
		}

		/// <summary>
		/// 綠界付款通知DTO
		/// </summary>
		public class EcpayCallbackDto
		{
			public string MerchantID { get; set; }
			public string MerchantTradeNo { get; set; }
			public string PaymentDate { get; set; }
			public string PaymentType { get; set; }
			public string PaymentTypeChargeFee { get; set; }
			public string RtnCode { get; set; }
			public string RtnMsg { get; set; }
			public string SimulatePaid { get; set; }
			public string TradeAmt { get; set; }
			public string TradeDate { get; set; }
			public string TradeNo { get; set; }
			public string CheckMacValue { get; set; }
		}



	}
}
