using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminUserListVM
    {
        [Display(Name = "編號")]
        public string UserId { get; set; }


        [Display(Name = "名稱")]
        public string Name { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "聯絡電話")]
        public string Phone { get; set; }


        [Display(Name ="狀態")]
        public string Status { get; set; }

        [Display(Name = "註記")]
        public string Note { get; set; }

        [Display(Name = "最後登入")]
        public DateTime? lastLoginAt { get; set; }

        [Display(Name = "最後執行動作")]
        public DateTime? lastAdminActionAt { get; set; }


        [Display(Name = "身分")]
        public string[] Roles { get; set; }

    }
}
