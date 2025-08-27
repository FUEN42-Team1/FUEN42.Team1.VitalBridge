using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ContentCategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class ContentCategoryCreateViewModel
    {
        [Display(Name = "名稱")]
        public string Name { get; set; }

        [Display(Name = "啟用")]
        public bool IsEnabled { get; set; }

        [Display(Name = "順序")]
        public int DisplayOrder { get; set; }
        [Display(Name = "父類別")]
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }  // For showing the name in initial <option>
        public List<ContentCategoryDTO>? ListCategory { get; set; }
    }

    public class ContentCategoryEditViewModel
    {
        public int Id { get; set; }
        [Display(Name = "名稱")]
        public string Name { get; set; }
        [Display(Name = "啟用")]
        public bool IsEnabled { get; set; }
        [Display(Name = "順序")]
        public int DisplayOrder { get; set; }
        [Display(Name = "父類別")]
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }  // For showing the name in initial <option>
        public List<ContentCategoryDTO>? ListCategory { get; set; }
    }
}
