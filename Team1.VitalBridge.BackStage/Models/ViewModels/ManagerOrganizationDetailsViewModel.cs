using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ManagerOrganizationDetailsViewModel
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "機構名稱")]
        public string Name { get; set; }

        [Display(Name = "機構照片")]
        public string? PhotoUrl { get; set; }

        [Display(Name = "縣市")]
        public int CityId { get; set; }
        public string? CityName { get; set; }

        [Display(Name = "鄉鎮區")]
        public int DistrictId { get; set; }
        public string? DistrictName { get; set; }

        [Display(Name = "地址")]
        public string Address { get; set; }

        [Display(Name = "機構類型")]
        public int TypeId { get; set; }
        public string? TypeName { get; set; }

        [Display(Name = "床位數量")]
        public int BedCount { get; set; }

        [Display(Name = "年齡限制")]
        public string? AgeLimits { get; set; }

        [Display(Name = "機構描述")]
        public string? Description { get; set; }

        [Display(Name = "地圖網址")]
        public string? MapUrl { get; set; }

        [Display(Name = "補貼資訊")]
        public List<string> SubsidyInfoDescription { get; set; } = new List<string>();

        [Display(Name = "特色服務")]
        public List<string> FeatureServiceNames { get; set; } = new List<string>();

        [Display(Name = "服務對象")]
        public List<string> ServiceTargetNames { get; set; } = new List<string>();

        [DisplayName("房型與價格")]
        public List<OrganizationRoomViewModel> Rooms { get; set; } = new List<OrganizationRoomViewModel>();

        [Display(Name = "所屬機構")] // 新增
        public string? InstitutionName { get; set; } // 新增

        [DisplayName("是否啟用")]
        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }
    }
}
