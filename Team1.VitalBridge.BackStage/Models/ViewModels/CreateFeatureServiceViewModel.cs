using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class CreateFeatureServiceViewModel
    {
        [Display(Name = "特色服務名稱")]
        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "服務圖片")]
        [Required(ErrorMessage = "請上傳{0}")]
        public string ImageUrl { get; set; }
    }
}
