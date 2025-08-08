using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class AdminLoginViewModel
    {
        //登入

        [Display(Name = "帳號")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string Email { get; set; }

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入{0}")]
        [DataType(DataType.Password)]
        public string Password { get; set; }



        //public string ReturnUrl { get; set; } = "/"; // 預設返回首頁
        //public bool RememberMe { get; set; } = false; // 是否記住登入狀態
        //public string ErrorMessage { get; set; } = string.Empty; // 用於顯示錯誤訊息
        //public string? Captcha { get; set; } // 用於驗證碼，如果有實作驗證碼功能




    }
}
