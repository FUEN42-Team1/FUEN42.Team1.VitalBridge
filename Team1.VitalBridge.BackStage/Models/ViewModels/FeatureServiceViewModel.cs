using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class FeatureServiceViewModel
    {
        public int Id { get; set; }

        [Display(Name = "特色服務名稱")]
        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "服務圖片")]
        [Required(ErrorMessage = "請上傳{0}")]
        public string ImageUrl { get; set; }

        [Display(Name = "啟用狀態")]
        public bool IsActive { get; set; }

        // 新增 FileId 屬性，用來儲存圖片檔案的 ID
        public int? FileId { get; set; }
    }
}