using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Institutions
{
    public class InstitutionRegisterLoginViewModel
    {

        [Display(Name = "機構代碼")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string InstitutionCode { get; set; }

        [Display(Name = "機構名稱")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string InstitutionName { get; set; }

        [Display(Name = "機構Email")]
        [Required(ErrorMessage = "請輸入{0}")]
        //[EmailAddress(ErrorMessage = "請輸入有效的Email地址")]
        public string InstitutionEmail { get; set; }

        [Display(Name = "機構電話")]
        [Required(ErrorMessage = "請輸入{0}")]
        [RegularExpression(@"^[0-9]{7,10}$", ErrorMessage = "電話格式錯誤，只允許7到10位的純數字")]
        public string InstitutionPhone { get; set; }

        [Display(Name = "機構負責人姓名")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string PrincipalName { get; set; }

        [Display(Name = "機構負責人電話")]
        [Required(ErrorMessage = "請輸入{0}")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機格式錯誤")]
        public string PrincipalPhone { get; set; }

        [Display(Name = "縣市")]
        public int CityId { get; set; } // 儲存選取的縣市 ID

        [Display(Name = "縣市名稱")]
        public string? CityName { get; set; } // 儲存縣市名稱，方便顯示

        [Display(Name = "鄉鎮")]
        public int TownshipId { get; set; } // 儲存選取的鄉鎮 ID

        [Display(Name = "鄉鎮名稱")]
        public string? TownshipName { get; set; } // 儲存鄉鎮名稱，方便顯示

        [Display(Name = "詳細地址")]
        public string? Address { get; set; }

        //帳號/密碼/確認密碼


        [Display(Name = "使用者名稱")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string Name { get; set; } 

        [Display(Name = "帳號信箱")]
        [Required(ErrorMessage = "請輸入{0}")]
        //[EmailAddress(ErrorMessage = "請輸入有效的Email地址")]
        public string Email { get; set; }
        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入{0}")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "確認密碼")]
        [Required(ErrorMessage = "請輸入{0}")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密碼和確認密碼不一致")]
        public string ConfirmPassword { get; set; }
    }
}
