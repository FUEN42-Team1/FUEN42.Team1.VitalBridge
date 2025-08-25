namespace Team1.VitalBridge.Frontend.Models.DTOs.Member
{
    public class UpdateProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int CityId { get; set; }
        public int TownshipId { get; set; }
        public string Address { get; set; } = string.Empty;
    }
}
