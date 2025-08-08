using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Institutions
{
    public class InstitutionLoginViewModel
    {
        [Display(Name = "機構代碼")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string InstitutionCode { get; set; }
        [Display(Name = "帳號信箱")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string Email { get; set; }

        [Display(Name = "密碼")]
        [Required(ErrorMessage = "請輸入{0}")]
        [DataType(DataType.Password)]
        public string Password { get; set; }


    }
}
