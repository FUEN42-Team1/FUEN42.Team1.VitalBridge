namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminInstitutionCreateVM
    {
        public string InstitutionCode { get; set; }
        public string InstitutionName { get; set; }
        public string InstitutionEmail { get; set; }
        public string InstitutionPhone { get; set; }
        public string PrincipalName { get; set; }
        public string PrincipalPhone { get; set; }
        public int CityId { get; set; }
        public string CityName { get; set; }
        public int TownshipId { get; set; }
        public string TownshipName { get; set; }
        public string Address { get; set; }
        public bool IsPhysicalCheck { get; set; }
        public bool IsBanned { get; set; }
        public string Status { get; set; }







    }
}
