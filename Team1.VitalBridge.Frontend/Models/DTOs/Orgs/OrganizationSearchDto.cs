namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 機構搜尋參數DTO
    /// </summary>
    public class OrganizationSearchDto
    {
        /// <summary>
        /// 關鍵字搜尋 (機構名稱)
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 縣市ID
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// 鄉鎮區ID
        /// </summary>
        public int? DistrictId { get; set; }

        /// <summary>
        /// 機構類型ID列表 (多選)
        /// </summary>
        public List<int>? OrganizationTypeIds { get; set; }

        /// <summary>
        /// 最低價格
        /// </summary>
        public int? MinPrice { get; set; }

        /// <summary>
        /// 最高價格
        /// </summary>
        public int? MaxPrice { get; set; }

        /// <summary>
        /// 特色服務ID列表 (多選)
        /// </summary>
        public List<int>? FeatureServiceIds { get; set; }

        /// <summary>
        /// 頁數 (預設為1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁大小 (預設為10)
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}