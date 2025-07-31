using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class AddBoxViewModel
    {
        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(50)]
        [Display(Name ="板塊名稱")]
        public string Name { get; set; }

        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(50)]
        [Display(Name = "路徑")]
        public string Route { get; set; }

        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(200)]
        [Display(Name = "位置")]
        public string Location { get; set; }

        [StringLength(50)]
        [Display(Name = "說明文字")]
        public string? DefualImageIntroduct { get; set; }

        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(200)]
        [Display(Name = "預設圖片檔案名稱")]
        public string DefualImageUrl { get; set; }

        [StringLength(200)]
        [Display(Name = "廣告連結")]
        public string? DefualImageClickUrl { get; set; }
    }
}
