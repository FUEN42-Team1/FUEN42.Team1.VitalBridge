using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ManagerOrganizationFormViewModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "機構名稱")]
        [Required(ErrorMessage = "{0} 為必填")]
        [StringLength(100, ErrorMessage = "{0} 長度不能超過100字元")]
        public string Name { get; set; }

        [Display(Name = "機構照片 (檔案上傳)")]
        [DataType(DataType.Upload)] // 標示為上傳檔案
        [NotMapped] 
        public IFormFile? PhotoFile { get; set; } // 用於接收上傳的檔案

        [Display(Name = "機構照片 (顯示路徑)")]
        public string? PhotoUrl { get; set; } // 用於顯示現有圖片的路徑

        [Display(Name = "縣市")]
        [Required(ErrorMessage = "{0} 為必選")]
        public int CityId { get; set; }
        public string? CityName { get; set; } // 顯示用

        [Display(Name = "鄉鎮區")]
        [Required(ErrorMessage = "{0} 為必選")]
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; } // 顯示用

        [Display(Name = "地址")]
        [Required(ErrorMessage = "{0} 為必填")]
        [StringLength(100, ErrorMessage = "{0} 長度不能超過100字元")]
        public string Address { get; set; }

        [Display(Name = "機構類型")]
        [Required(ErrorMessage = "{0} 為必選")]
        public int TypeId { get; set; }
        public string? TypeName { get; set; } // 顯示用

        [Display(Name = "床位數量")]
        [Required(ErrorMessage = "{0} 為必填")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} 必須為非負數")]
        public int BedCount { get; set; }

        [Display(Name = "年齡限制")]
        [StringLength(100, ErrorMessage = "{0} 長度不能超過100字元")]
        public string? AgeLimits { get; set; }

        [Display(Name = "機構描述")]
        [StringLength(1000, ErrorMessage = "{0} 長度不能超過1000字元")]  // 從 100 改為 1000
        public string? Description { get; set; }

        [Display(Name = "地圖網址")]
        [StringLength(100, ErrorMessage = "{0} 長度不能超過100字元")]
        public string? MapUrl { get; set; }

        // 多對多關係名稱列表 (用於顯示，Details)
        [Display(Name = "補貼資訊")]
        public List<string> SubsidyInfoNames { get; set; } = new List<string>();

        [Display(Name = "特色服務")]
        public List<string> FeatureServiceNames { get; set; } = new List<string>();

        [Display(Name = "服務對象")]
        public List<string> ServiceTargetNames { get; set; } = new List<string>();

        // 統一使用此列表來處理所有房間相關數據
        [DisplayName("房型與價格")]
        public List<OrganizationRoomViewModel> Rooms { get; set; } = new List<OrganizationRoomViewModel>();

        // 為了 Create/Edit 表單提交時接收多對多關係的選擇ID
        [DisplayName("政府補助")]
        public List<int> SelectedSubsidyInfoIds { get; set; } = new List<int>();

        [DisplayName("特色服務")]
        public List<int> SelectedFeatureServiceIds { get; set; } = new List<int>();

        [DisplayName("服務對象")]
        public List<int> SelectedServiceTargetIds { get; set; } = new List<int>();

        /// <summary>
        /// 用於接收從前端傳回的要刪除的房型ID列表 (JSON 字串)
        /// </summary>
        public string? RemovedRoomIds { get; set; }

        [DisplayName("是否啟用")]
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        [Display(Name = "所屬機構")]
        public int? InstitutionId { get; set; }
        public string? InstitutionName { get; set; }

        
        public int? FileId { get; set; } // 用於存儲上傳的檔案ID
    }
}