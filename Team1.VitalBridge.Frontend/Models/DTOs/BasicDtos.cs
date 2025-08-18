namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    /// <summary>
    /// 城市 DTO
    /// </summary>
    public class CityDto
    {
        /// <summary>
        /// 城市ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 城市名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 鄉鎮區 DTO
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
    }

    /// <summary>
    /// 機構類型 DTO
    /// </summary>
    public class OrganizationTypeDto
    {
        /// <summary>
        /// 機構類型ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 機構類型名稱
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}