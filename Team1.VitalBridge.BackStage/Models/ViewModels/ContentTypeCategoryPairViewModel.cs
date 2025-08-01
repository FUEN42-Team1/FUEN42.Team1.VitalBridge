using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        public int ParentContentCategoryDisplayOrder { get; set; }

        [Display(Name = "Content Category Name")]
        public string ContentCategoryName { get; set; }

        [Display(Name = "CC Order")]
        public int ContentCategoryDisplayOrder { get; set; }

        [Display(Name = "Status")]
        public string IsEnabledStatus { get; set; }
    }

    public class ContentTypeCategoryPairCreateViewModel
    {
        [ScaffoldColumn(false)]
        public int Id { get; set; } // For editing existing pairs, or 0 for new pairs

        // For selecting an existing ContentType
        [Display(Name = "Content Type")]
        [Required(ErrorMessage = "Please select a Content Type.")]
        public int ContentTypeId { get; set; }

        // For selecting an existing ContentCategory OR indicating a new one will be created
        [Display(Name = "Existing Content Category")]
        public int? ContentCategoryId { get; set; } // Nullable, as a new category might be created

        // For creating a new ContentCategory if ContentCategoryId is null
        [Display(Name = "New Category Name")]
        [StringLength(50, ErrorMessage = "New Category Name cannot exceed 50 characters.")]
        public string NewCategoryName { get; set; }

        // For the parent of the new ContentCategory (optional)
        [Display(Name = "New Category Parent (Optional)")]
        public int? NewCategoryParentCategoryId { get; set; }

        // Properties for the new ContentCategory if created
        [Display(Name = "Is New Category Enabled?")]
        public bool IsNewCategoryEnabled { get; set; } = true; // Default to true

        [Display(Name = "New Category Display Order")]
        public int NewCategoryDisplayOrder { get; set; } = 0; // Default value

        // For the ContentTypeCategoryPair itself
        [Display(Name = "Is Pair Enabled?")]
        public bool IsEnabled { get; set; } = true; // Default to true

        // Properties for dropdowns (will be populated via AJAX)
        // These are typically populated by the controller action, but for AJAX,
        // the client-side will fetch the data and build the dropdowns.
        // However, it's good practice to have them here for potential server-side rendering
        // or for clarity on what data is expected.
        //public IEnumerable<SelectListItem> AvailableContentTypes { get; set; }
        //public IEnumerable<SelectListItem> AvailableContentCategories { get; set; }
    }

    public class ContentTypeCategoryPairEditViewModel
    {
    }
}
