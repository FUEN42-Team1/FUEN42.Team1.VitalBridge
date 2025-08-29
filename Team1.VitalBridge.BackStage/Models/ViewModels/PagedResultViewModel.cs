namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	/// <summary>
	/// 通用分頁結果 ViewModel
	/// 包含資料列表和分頁相關資訊
	/// </summary>
	/// <typeparam name="T">資料項目類型</typeparam>
	public class PagedResultViewModel<T>
	{
		/// <summary>
		/// 當前頁的資料項目列表
		/// </summary>
		public List<T> Items { get; set; } = new List<T>();

		/// <summary>
		/// 符合條件的總資料筆數
		/// </summary>
		public int TotalItems { get; set; }

		/// <summary>
		/// 當前頁碼（從 1 開始）
		/// </summary>
		public int CurrentPage { get; set; }

		/// <summary>
		/// 每頁顯示筆數
		/// </summary>
		public int PageSize { get; set; }

		/// <summary>
		/// 總頁數
		/// </summary>
		public int TotalPages { get; set; }

		/// <summary>
		/// 是否有上一頁
		/// </summary>
		public bool HasPreviousPage => CurrentPage > 1;

		/// <summary>
		/// 是否有下一頁
		/// </summary>
		public bool HasNextPage => CurrentPage < TotalPages;

		/// <summary>
		/// 當前頁的起始項目編號（用於顯示 "顯示第 X 到第 Y 項"）
		/// </summary>
		public int StartItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;

		/// <summary>
		/// 當前頁的結束項目編號
		/// </summary>
		public int EndItem => Math.Min(CurrentPage * PageSize, TotalItems);

		/// <summary>
		/// 建構子：計算分頁相關數值
		/// </summary>
		/// <param name="items">當前頁資料</param>
		/// <param name="totalItems">總資料筆數</param>
		/// <param name="currentPage">當前頁碼</param>
		/// <param name="pageSize">每頁筆數</param>
		public PagedResultViewModel(List<T> items, int totalItems, int currentPage, int pageSize)
		{
			Items = items;
			TotalItems = totalItems;
			CurrentPage = currentPage;
			PageSize = pageSize;

			// 計算總頁數（無條件進位）
			TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
		}

		/// <summary>
		/// 預設建構子
		/// </summary>
		public PagedResultViewModel()
		{
		}
	}
}
