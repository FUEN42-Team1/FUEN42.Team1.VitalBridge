using Team1.VitalBridge.BackStage.Models.EFModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class AdminPlateViewModel
    {

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "板塊名稱")]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "板塊位置")]
        public string Route { get; set; }

        [Display(Name = "啟用")]
        public bool Enable { get; set; }
    }
}
