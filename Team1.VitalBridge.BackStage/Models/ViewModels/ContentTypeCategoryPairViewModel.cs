using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ContentTypeCategoryPairViewModel
    {
    }

    public class ContentTypeCategoryPairDisplayViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Content Type Name")]
        public string ContentTypeName { get; set; }

        [Display(Name = "CT Order")]
        public int ContentTypeDisplayOrder { get; set; }

        [Display(Name = "Parent Content Category Name")]
        public string ParentContentCategoryName { get; set; }

        [Display(Name = "Parent CC Order")]
        public int? ParentContentCategoryDisplayOrder { get; set; }

        [Display(Name = "Content Category Name")]
        public string ContentCategoryName { get; set; }

        [Display(Name = "CC Order")]
        public int ContentCategoryDisplayOrder { get; set; }

        [Display(Name = "Status")]
        public string IsEnabledStatus { get; set; }
    }

    public class ContentTypeCategoryPairCreateViewModel
    {
    }

    public class ContentTypeCategoryPairEditViewModel
    {
    }
}
