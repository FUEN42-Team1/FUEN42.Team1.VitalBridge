namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 機構特色服務DTO
    /// </summary>
    public class FeatureServiceDto
    {
        /// <summary>
        /// 特色服務ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 特色服務名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 圖片檔名 (用於 /api/UploadFile/GetFile?fileName=圖片檔名)
        /// </summary>
        public string? ImageUrl { get; set; }
    }
}