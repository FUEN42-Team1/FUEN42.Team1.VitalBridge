namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class PaginationViewModel
	{
		// 當前頁碼
		public int CurrentPage { get; set; } = 1;

		// 每頁筆數
		public int PageSize { get; set; } = 10;

		// 總筆數
		public int TotalCount { get; set; }

		// 總頁數
		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

		// 是否有上一頁
		public bool HasPreviousPage => CurrentPage > 1;

		// 是否有下一頁
		public bool HasNextPage => CurrentPage < TotalPages;

		/// <summary>
		/// 當前頁開始筆數
		/// </summary>
		public int StartIndex => (CurrentPage - 1) * PageSize + 1;

		/// <summary>
		/// 當前頁結束筆數
		/// </summary>
		public int EndIndex => Math.Min(CurrentPage * PageSize, TotalCount);

	}
}