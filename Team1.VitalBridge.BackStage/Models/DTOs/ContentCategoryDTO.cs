namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class ContentCategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentCategoryId { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
