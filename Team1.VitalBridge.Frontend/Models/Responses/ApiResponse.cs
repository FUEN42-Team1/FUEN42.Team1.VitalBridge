namespace Team1.VitalBridge.Frontend.Models.Responses
{
    /// <summary>
    /// 統一 API 回應格式
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 回應訊息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 實際資料
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 錯誤代碼 (可選)
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// 時間戳記
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 建立成功回應
        /// </summary>
        public static ApiResponse<T> CreateSuccess(T data, string message = "操作成功")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// 建立失敗回應
        /// </summary>
        public static ApiResponse<T> CreateError(string message, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }

    /// <summary>
    /// 分頁回應資料
    /// </summary>
    /// <typeparam name="T">資料類型</typeparam>
    public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
    {
        /// <summary>
        /// 總筆數
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 當前頁碼
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 每頁筆數
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
        /// 建立分頁成功回應
        /// </summary>
        public static PagedApiResponse<T> CreatePagedSuccess(
            IEnumerable<T> data, 
            int totalCount, 
            int currentPage, 
            int pageSize, 
            string message = "查詢成功")
        {
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            return new PagedApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                TotalCount = totalCount,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }
}