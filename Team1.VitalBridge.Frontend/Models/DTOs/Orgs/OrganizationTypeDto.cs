namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 機構類型DTO
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