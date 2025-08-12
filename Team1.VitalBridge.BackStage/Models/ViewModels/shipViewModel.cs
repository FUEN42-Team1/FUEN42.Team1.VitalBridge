using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class shipViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "運送方式名稱")]
        public string ShipMethodName { get; set; }

        [Required]
		[DataType(DataType.Currency)]
		[Display(Name = "運費")]
        public decimal ShipCost { get; set; }

        [Display(Name = "啟用狀態")]
        public bool? IsActive { get; set; }
    }
}
