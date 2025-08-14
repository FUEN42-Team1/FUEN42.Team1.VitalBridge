using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminEditVM
    {
        public string UserId { get; set; }

        public string Name { get; set; } // 使用者名稱

        [RegularExpression(@"^09\d{8}$", ErrorMessage = "請輸入合法的手機號碼")]
        public string? Phone { get; set; } // 聯絡電話
        [StringLength(250)]
        public string? Note { get; set; } // 註記

    }
}
