using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminUserDetailsVM
    {

        [Display(Name = "編號")]
        public string UserId { get; set; }


        [Display(Name = "名稱")]
        public string Name { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "聯絡電話")]
        public string? Phone { get; set; }


        [Display(Name = "狀態")]
        public string Status { get; set; }

        [Display(Name = "註記")]
        public string? Note { get; set; }
        


        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }

        [Display(Name = "最後登入")]
        public DateTime? LastLoginAt { get; set; }

        [Display(Name = "最後執行動作")]
        public DateTime? LastAdminActionAt { get; set; }

        [Display(Name = "當前累計登入失敗")]
        public int? FailedLoginCount { get; set; }

        [Display(Name = "鎖定到期")]
        public DateTime? LockedUntil { get; set; }



        [Display(Name = "身分")]
        public string[] Roles { get; set; }
    }
}
