namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class ContentTypeCategoryPairDTO
    {
    }

    public class ContentTypeCategoryPairDisplayDTO
    {
        public int Id { get; set; }

        // Content Type
        public int ContentTypeId { get; set; }
        public string ContentTypeName { get; set; }
        public int ContentTypeDisplayOrder { get; set; }

        // Content Category
        public string? ParentContentCategoryName { get; set; }
        public int? ParentContentCategoryDisplayOrder { get; set; }

        public int ContentCategoryId { get; set; }
        public string ContentCategoryName { get; set; }
        public int ContentCategoryDisplayOrder { get; set; }

        public bool IsEnabled { get; set; }
    }

    public class ContentTypeCategoryPairCreateDTO
    {
    }

    public class ContentTypeCategoryPairEditDTO
    {
    }
}
