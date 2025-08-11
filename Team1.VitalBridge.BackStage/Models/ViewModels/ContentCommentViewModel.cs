namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ContentCommentViewModel
    {
    }

    public class ContentCommentSearchViewModel
    {
        public string? SearchContent { get; set; }
        public string? SearchMemberName { get; set; }
        public string? SearchContentTitle { get; set; }
        public int? ParentCommentId { get; set; }
        public bool? IsPinned { get; set; }
        public DateTime? CreatedAtStart { get; set; }
        public DateTime? CreatedAtEnd { get; set; }

    }

    public class ContentCommentListViewModel
    {
        public int Id { get; set; }
        public string MemberName { get; set; }
        public string ContentTitle { get; set; }
        public int? ParentCommentId { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
