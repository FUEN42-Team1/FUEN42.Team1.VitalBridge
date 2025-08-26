namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
	public class CartResponseDto
	{
		public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
		public int TotalQuantity { get; set; }
		public decimal TotalAmount { get; set; }

	}
}
