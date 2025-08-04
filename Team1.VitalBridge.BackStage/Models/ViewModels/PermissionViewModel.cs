namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class PermissionViewModel
    {
        public string PermissionCode { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}
