namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
	public class UpdateCartItemDto
	{
		public int CartItemId { get; set; }  // CartItem 的 ID
		public int Quantity { get; set; }    // 新的數量
	}
}
