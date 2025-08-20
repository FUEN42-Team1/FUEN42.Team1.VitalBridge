namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 縣市DTO
    /// </summary>
    public class CityDto
    {
        /// <summary>
        /// 縣市ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 縣市名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}