using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminInstitutionListVM
    {
        //public string Id { get; set; } // 機構ID


        [Display(Name = "機構代碼")]
        public string InstitutionCode { get; set; }

        [Display(Name = "機構名稱")]
        public string InstitutionName { get; set; }

        [Display(Name = "機構Email")]
        public string InstitutionEmail { get; set; }

        [Display(Name = "機構電話")]
        public string InstitutionPhone { get; set; }

        [Display(Name = "機構負責人姓名")]
        public string PrincipalName { get; set; }

        [Display(Name = "機構負責人電話")]
        public string PrincipalPhone { get; set; }

        public string Status { get; set; } // 機構狀態

        [Display(Name = "實地確認")]
        public bool IsPhysicalCheck { get; set; } // 是否實體機構檢查
        
        [Display(Name = "是否停權")]
        public bool IsBanned { get; set; }


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

    }
}
