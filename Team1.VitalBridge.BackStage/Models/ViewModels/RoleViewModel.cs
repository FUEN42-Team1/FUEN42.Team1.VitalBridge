using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class RoleViewModel
    {
        [Required]
        [Display(Name = "身分代碼")]
        public string RoleCode { get; set; }
        [Required]
        [Display(Name = "身分名稱")]
        public string Name { get; set; }

        [Display(Name = "說明")]
        public string? Info { get; set; }

        [Required]
        [Display(Name = "是否啟用此身分")]
        public bool isActive { get; set; } // 是否啟用此身分
        [Required]
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }
        [Required]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; } 
    }
}
