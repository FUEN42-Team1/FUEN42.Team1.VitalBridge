namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductCategoryOptionViewModel
	{

		// 這是在商品建立用來顯示類別樹枝狀圖，前端顯示使用

		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsChecked { get; set; }  // 只在前端邏輯不會存資料庫，判斷有沒有打v

		public bool HasChildren { get; set; } // 是否有子分類（用於 AJAX 展開用）

		public List<ProductCategoryOptionViewModel> Children { get; set; } = new(); // 子分類
	}
}
