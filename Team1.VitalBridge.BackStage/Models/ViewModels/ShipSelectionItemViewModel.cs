namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ShipSelectionItemViewModel
	{
		// 要載入物流選項
		public int Id { get; set; }
		public string ShipMethodName { get; set; }
		public decimal ShipCost { get; set; }
		public bool IsActive { get; set; }
	}
}
