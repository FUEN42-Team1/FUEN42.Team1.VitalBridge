namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class OrderStatusOptionViewModel
	{
		// Tab按鈕的狀態選項

		// 狀態Id
		public int? StatusId { get; set; }

		// 狀態名稱
		public string StatusName { get; set; }

		// 該狀態訂單數量
		public int Count { get; set; }

		//是否為當前選中狀態
		public bool IsActive { get; set; }

	}
}