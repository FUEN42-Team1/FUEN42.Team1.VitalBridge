namespace Team1.VitalBridge.BackStage.Models.ViewModels.Institutions
{
    public class InstitutionClaimListItemVM
    {
        public int InstitutionId { get; set; }
        public string InstitutionCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string AddressDisplay { get; set; } = ""; // CityName + TownshipName + Address
        public string Status { get; set; } = "";
        public bool IsPhysicalCheck { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? Phone { get; set; }
    }
}
