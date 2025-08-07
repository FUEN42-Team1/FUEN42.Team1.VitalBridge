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
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }  // For showing the name in initial <option>
        public List<ContentCategoryDTO>? ListCategory { get; set; }
    }

    public class ContentCategoryEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }  // For showing the name in initial <option>
        public List<ContentCategoryDTO>? ListCategory { get; set; }
    }
}
