namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    public class SearchInfoDto
    {
        // 搜尋資訊 DTO
        public int? CategoryId { get; set; } // 可選的分類 ID
        public string CategoryName { get; set; } // 可選的分類名稱
        public string SearchQuery { get; set; } // 搜尋關鍵字
        public int TotalFound { get; set; }     // 總共找到的商品數量

    }
}
