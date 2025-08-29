using System.ComponentModel.DataAnnotations;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{

    public class RoleViewModel
    {
        [Required]
        [Display(Name = "身分代碼")]
        public string RoleCode { get; set; }
        [Required]
        [Display(Name = "身分名稱")]
        public string Name { get; set; }

        [Display(Name = "說明")]
        public string? Info { get; set; }

        [Display(Name = "類型")]
        [Required(ErrorMessage = "請選擇身分類型")]
        public RoleTypeItem RoleType { get; set; }

        [Required]
        [Display(Name = "是否啟用此身分")]
        public bool IsActive { get; set; } // 是否啟用此身分

        [Display(Name = "是否為系統預設身分")]
        public bool IsSystemDefault { get; set; } // 是否為系統預設身分

        [Required]
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }
        [Required]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }



        public enum RoleTypeItem
        {
            Member,        // 會員系統
            Institution,   // 機構後台
            Admin          // 管理員後台
        }

        public string RoleTypeName => RoleType.ToString();

        public string RoleTypeShowName { get; set; }

        //public List<int> SelectedPermissionIds { get; set; } = new();
        //public List<Permission> AllPermissions { get; set; } = new();
    }


    public class RoleCreateViewModel
    {
        [Required]
        [Display(Name = "身分代碼")]
        public string RoleCode { get; set; }
        [Required]
        [Display(Name = "身分名稱")]
        public string Name { get; set; }

        [Display(Name = "說明")]
        public string? Info { get; set; }

        [Display(Name = "類型")]
        [Required(ErrorMessage = "請選擇身分類型")]
        public RoleTypeItem RoleType { get; set; }

        [Required]
        [Display(Name = "是否啟用此身分")]
        public bool IsActive { get; set; } // 是否啟用此身分

        [Display(Name = "是否為系統預設身分")]
        public bool IsSystemDefault { get; set; } // 是否為系統預設身分

        [Required]
        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }
        [Required]
        [Display(Name = "更新時間")]
        public DateTime UpdatedAt { get; set; }



        public enum RoleTypeItem
        {
            Member,        // 會員系統
            Institution,   // 機構後台
            Admin          // 管理員後台
        }

        public string RoleTypeName => RoleType.ToString();


        //public List<int> SelectedPermissionIds { get; set; } = new();
        //public List<Permission> AllPermissions { get; set; } = new();
    }
}
