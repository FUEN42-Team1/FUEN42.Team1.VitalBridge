namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    /// <summary>
    /// 前端機構搜尋結果 DTO
    /// </summary>
    public class FrontendOrganizationResult
    {
        /// <summary>
        /// 機構ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 機構名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 機構類型名稱
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 機構類型ID
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// 城市名稱
        /// </summary>
        public string CityName { get; set; } = string.Empty;

        /// <summary>
        /// 鄉鎮區名稱
        /// </summary>
        public string DistrictName { get; set; } = string.Empty;

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// 床位數量
        /// </summary>
        public int BedCount { get; set; }

        /// <summary>
        /// 年齡限制
        /// </summary>
        public string? AgeLimits { get; set; }

        /// <summary>
        /// 機構描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Google 地圖連結
        /// </summary>
        public string? MapUrl { get; set; }

        /// <summary>
        /// 該機構最低月租價格
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// 特色服務名稱列表
        /// </summary>
        public List<string> FeatureServices { get; set; } = new List<string>();

        /// <summary>
        /// 機構照片URL
        /// </summary>
        public string? PhotoUrl { get; set; }
    }
}