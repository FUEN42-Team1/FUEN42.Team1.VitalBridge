namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 機構列表項目DTO
    /// </summary>
    public class OrganizationListItemDto
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
        /// 縣市名稱
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
        /// 床位數
        /// </summary>
        public int BedCount { get; set; }

        /// <summary>
        /// 機構照片檔名 (用於 /api/UploadFile/GetFile?fileName=圖片檔名)
        /// </summary>
        public string? PhotoUrl { get; set; }

        /// <summary>
        /// 機構類型名稱
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 最低月費 (從房型中取得)
        /// </summary>
        public int? MinMonthlyPrice { get; set; }

        /// <summary>
        /// 是否推薦
        /// </summary>
        public bool? IsRecommended { get; set; }

        /// <summary>
        /// 是否認證
        /// </summary>
        public bool? IsCertified { get; set; }

        /// <summary>
        /// 特色服務列表 (只顯示名稱)
        /// </summary>
        public List<string> FeatureServiceNames { get; set; } = new List<string>();
    }
}