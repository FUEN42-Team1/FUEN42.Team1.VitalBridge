namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class ContentCommentDTO
    {
    }
    public class ContentCommentDisplayDTO
    {
        public int Id { get; set; }
        public string MemberName { get; set; }
        public string ContentTitle { get; set; }
        public int? ParentCommentId { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ContentCommentListCritriaDTO
    {
        public string? ContentTitle { get; set; }
        public bool? IsPinned { get; set; }
        public DateTime? CreatedAtFrom { get; set; }
        public DateTime? CreatedAtTo { get; set; }
    }

}
