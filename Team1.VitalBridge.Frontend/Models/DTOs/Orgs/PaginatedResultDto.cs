namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 分頁結果DTO
    /// </summary>
    /// <typeparam name="T">項目類型</typeparam>
    public class PaginatedResultDto<T>
    {
        /// <summary>
        /// 項目列表
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 當前頁數
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// 每頁大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 總頁數
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有上一頁
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// 是否有下一頁
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }
}