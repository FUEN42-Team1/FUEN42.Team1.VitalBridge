namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    public class PaginationDto
    {
        // 分頁資訊 DTO

        public int CurrentPage { get; set; } // 當前頁碼
        public int TotalPages { get; set; } // 總頁數
        public int TotalItems { get; set; } // 總項目數
        public int PageSize { get; set; }  // 每頁項目數
        public bool HasNextPage { get; set; } // 是否有下一頁
        public bool HasPreviousPage { get; set; } // 是否有上一頁

    }
}
