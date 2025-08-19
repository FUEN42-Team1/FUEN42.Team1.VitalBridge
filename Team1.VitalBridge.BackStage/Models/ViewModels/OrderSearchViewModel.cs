using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class OrderSearchViewModel
	{
		// 這是點選訂單列表頁面時，選擇的搜尋條件模型


		// 搜尋關鍵字

		[Display(Name = "搜尋訂單號碼、會員名稱")]
		public string Keyword { get; set; }

		// 訂單狀態篩選（null = 全部）
	
		public int? StatusFilter { get; set; }

		// 每頁顯示筆數

		public int PageSize { get; set; } = 10;

		
		// 當前頁碼
		
		public int Page { get; set; } = 1;


	}
}