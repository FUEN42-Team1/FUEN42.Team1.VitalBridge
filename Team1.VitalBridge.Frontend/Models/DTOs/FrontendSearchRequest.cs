using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    /// <summary>
    /// 前端機構搜尋請求 DTO
    /// </summary>
    public class FrontendSearchRequest
    {
        /// <summary>
        /// 機構名稱或地址關鍵字
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 城市ID
        /// </summary>
        public int? CityId { get; set; }

        /// <summary>
        /// 鄉鎮區ID
        /// </summary>
        public int? DistrictId { get; set; }

        /// <summary>
        /// 最高月租價格
        /// </summary>
        [Range(0, 1000000, ErrorMessage = "價格必須大於等於 0")]
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// 機構類型ID陣列
        /// </summary>
        public List<int> OrganizationTypes { get; set; } = new List<int>();

        /// <summary>
        /// 頁碼（預設為 1）
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "頁碼必須大於 0")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每頁筆數（預設為 20，與前台一致）
        /// </summary>
        [Range(1, 100, ErrorMessage = "每頁筆數必須在 1-100 之間")]
        public int PageSize { get; set; } = 20;
    }
}