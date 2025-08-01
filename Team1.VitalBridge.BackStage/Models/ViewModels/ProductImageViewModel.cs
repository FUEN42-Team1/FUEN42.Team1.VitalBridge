namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductImageViewModel
	{
		public int Id { get; set; }   // 對應資料庫主鍵
		public string ImageUrl { get; set; }  // 圖片路徑，前端載入使用

		public int? SortOrder { get; set; } // 排序順序，拖曳排序用
	}
}
