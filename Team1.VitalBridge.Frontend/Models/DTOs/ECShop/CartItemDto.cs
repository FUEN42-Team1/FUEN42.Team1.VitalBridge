namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
	public class CartItemDto
	{
		public int Id { get; set; }  // CartItem 的 ID
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public decimal UnitPrice { get; set; }
		public string ItemNumber { get; set; }  // 🔥 新增：商品貨號
		public int Quantity { get; set; }
		public decimal Subtotal { get; set; }
		public string ImageFileName { get; set; }  // 商品圖片
		public int Stock { get; set; }  // 目前庫存（用來檢查是否足夠）

	}
}
