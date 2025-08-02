using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Code { get; set; } // 接收確認碼

        [Required]
        public string uid { get; set; } // 接收使用者 ID


        [Required]
        [Display(Name = "新密碼")]
        [DataType(DataType.Password)]
        //[StringLength(100, ErrorMessage = "密碼長度必須在 {2} 到 {1} 個字元之間。", MinimumLength = 6)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "確認新密碼")]
        [Compare("NewPassword", ErrorMessage = "新密碼和確認新密碼不一致。")]
        public string ConfirmNewPassword { get; set; }

            
    }
}
