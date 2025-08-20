namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 機構詳細資料DTO
    /// </summary>
    public class OrganizationDetailDto
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
        /// 完整地址
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
        /// 機構介紹
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Google 地圖連結
        /// </summary>
        public string? MapUrl { get; set; }

        /// <summary>
        /// 收容年齡限制
        /// </summary>
        public string? AgeLimits { get; set; }

        /// <summary>
        /// 是否推薦
        /// </summary>
        public bool? IsRecommended { get; set; }

        /// <summary>
        /// 是否認證
        /// </summary>
        public bool? IsCertified { get; set; }

        /// <summary>
        /// 特色服務列表 (含圖片)
        /// </summary>
        public List<FeatureServiceDto> FeatureServices { get; set; } = new List<FeatureServiceDto>();

        /// <summary>
        /// 服務對象列表
        /// </summary>
        public List<string> ServiceTargetNames { get; set; } = new List<string>();

        /// <summary>
        /// 房型資訊列表
        /// </summary>
        public List<RoomInfoDto> Rooms { get; set; } = new List<RoomInfoDto>();

        /// <summary>
        /// 補助資訊描述列表
        /// </summary>
        public List<string> SubsidyInfoDescriptions { get; set; } = new List<string>();
    }
}