using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.Dto
{
    public class UpdatePlateImageDto
    {
        public int Id { get; set; }

        [StringLength(50)]
        [Display(Name = "說明文字")]
        public string? Introduct { get; set; }

        [Required(ErrorMessage = "請選擇{0}")]
        [Display(Name = "圖片檔案名稱")]
        [StringLength(200)]
        public string ImageFileName { get; set; }

        [Required(ErrorMessage = "請選擇{0}")]
        [Display(Name = "開始時間")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "請選擇{0}")]
        [Display(Name = "結束時間")]
        public DateOnly EndDate { get; set; }


        [Column("clickUrl")]
        [Display(Name = "點擊連結")]
        [StringLength(500)]
        public string? ClickUrl { get; set; }
    }
}
