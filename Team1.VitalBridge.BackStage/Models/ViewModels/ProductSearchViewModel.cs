using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	/// <summary>
	/// 商品搜尋參數 ViewModel
	/// 用於接收前端傳來的搜尋、篩選、分頁參數
	/// </summary>
	public class ProductSearchViewModel
	{
		/// <summary>
		/// 搜尋關鍵字（可搜尋商品名稱和貨號）
		/// </summary>
		public string Keyword { get; set; } = "";

		/// <summary>
		/// 商品狀態篩選
		/// "all" = 全部商品, "active" = 上架中, "inactive" = 下架中
		/// </summary>
		public string Status { get; set; } = "all";

		/// <summary>
		/// 當前頁碼（從 1 開始）
		/// </summary>
		[Range(1, int.MaxValue)]
		public int Page { get; set; } = 1;

		/// <summary>
		/// 每頁顯示筆數
		/// </summary>
		[Range(1, 100)]
		public int PageSize { get; set; } = 10;
	}
}
