using System;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels.Admin
{
    public class AdminInstitutionDetailVM
    {
        [Display(Name = "機構代碼")]
        public string InstitutionCode { get; set; } = string.Empty;

        [Display(Name = "機構名稱")]
        public string InstitutionName { get; set; } = string.Empty;

        [Display(Name = "機構Email")]
        public string InstitutionEmail { get; set; } = string.Empty;

        [Display(Name = "機構電話")]
        public string InstitutionPhone { get; set; } = string.Empty;

        [Display(Name = "負責人姓名")]
        public string PrincipalName { get; set; } = string.Empty;

        [Display(Name = "負責人電話")]
        public string PrincipalPhone { get; set; } = string.Empty;

        [Display(Name = "狀態")]
        public string Status { get; set; } = "Pending"; // Pending | Approved | Rejected

        public bool IsPhysicalCheck { get; set; } // 是否實地勘查

        public bool IsBanned { get; set; } // 是否被封鎖

        [Display(Name = "縣市")]
        public string? CityName { get; set; }

        [Display(Name = "鄉鎮")]
        public string? TownshipName { get; set; }

        [Display(Name = "詳細地址")]
        public string? Address { get; set; }

        // 下面兩個是為了顯示許可證預覽（若你有檔案表可以之後換成從 DB 來）
        public string? PermitImageUrl { get; set; }   // e.g. /uploads/institutions/{code}/permit.jpg
        public DateTime? PermitUploadedAt { get; set; }
    }
}
