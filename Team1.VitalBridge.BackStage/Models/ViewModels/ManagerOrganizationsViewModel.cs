using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ManagerOrganizationsViewModel
    {
        [Display(Name = "編號")]
        public int Id { get; set; }

        [Display(Name = "機構名稱")]
        public string Name { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Display(Name = "床位數量")]
        public int BedCount { get; set; }

        [Display(Name = "是否啟用")]
        public bool IsActive { get; set; }
    }
}
