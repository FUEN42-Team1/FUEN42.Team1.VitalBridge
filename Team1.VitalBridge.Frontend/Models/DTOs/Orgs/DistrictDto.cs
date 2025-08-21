namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 鄉鎮區DTO
    /// </summary>
    public class DistrictDto
    {
        /// <summary>
        /// 鄉鎮區ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 鄉鎮區名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 所屬縣市ID
        /// </summary>
        public int CityId { get; set; }
    }
}