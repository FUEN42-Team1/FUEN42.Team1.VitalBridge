using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels;

public class FeatureServiceViewModel
{
    public int Id { get; set; }

    [Display(Name = "特色服務")]
    [Required(ErrorMessage = "請輸入{0}")]
    [StringLength(100)]
    public string Name { get; set; }

    // 用來儲存已上傳圖片的檔案名稱
    [Required(ErrorMessage = "請上傳圖片")]
    public string ImageUrl { get; set; }
}