using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "會員編號")]
        public string UserId { get; set; }

        [Display(Name ="姓名")]
        public string Name { get; set; }
        [Display(Name = "電子信箱")]
        public string Email { get; set; } 

        [Display(Name = "聯絡電話")]
        [RegularExpression(@"^09\d{8}$", ErrorMessage = "手機格式錯誤")]
        public string Phone { get; set; }



        [Display(Name = "縣市")]
        public int? CityId { get; set; } // 儲存選取的縣市 ID

        [Display(Name = "縣市名稱")]
        public string? CityName { get; set; } // 儲存縣市名稱，方便顯示

        [Display(Name = "鄉鎮")]
        public int? TownshipId { get; set; } // 儲存選取的鄉鎮 ID

        [Display(Name = "鄉鎮名稱")]
        public string? TownshipName { get; set; } // 儲存鄉鎮名稱，方便顯示

        [Display(Name = "詳細地址")]
        public string? Address { get; set; }

        [Display(Name = "狀態")]
        public string Status { get; set; } // 例如：Active, Inactive, Banned 等

        [Display(Name ="註冊時間")]

        public DateTime CreatedAt { get; set; } // 會員註冊時間
        [Display(Name = "最後更新")]
        public DateTime UpdatedAt { get; set; } // 會員資料最後更新時間

        [Display(Name = "最後登入")]
        public DateTime? LastLoginAt { get; set; } // 最後登入時間




        public List<int> SelectedRoleIds { get; set; } = new();
        public List<string> Roles { get; set; } = new();
    }
}
