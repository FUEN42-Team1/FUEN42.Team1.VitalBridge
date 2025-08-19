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

    public class ContentArticleCommentCriteriaDTO
    {
        public string? ArticleTitle { get; set; }      // filter by article title
        public string? MemberName { get; set; }        // filter by member name
        public string? CommentContent { get; set; }    // filter by comment content
        public string? Status { get; set; } = "all";   // "all", "enabled", "disabled"
    }

    public class ContentArticleCommentEditDTO
    {
        public int Id { get; set; } 
    }
    public class ContentArticleCommentDisplayDTO
    {
        public int Id { get; set; } // Hidden in the view
        public string ArticleTitle { get; set; } // Assuming this is not editable
        public int ArticleId { get; set; } // Assuming this is hidden in the view
        public string MemberName { get; set; } // Assuming this is not editable
        public int MemberId { get; set; } // Assuming this is hidden in the view
        public int? ParentCommentId { get; set; } // Assuming this is not editable
        public string CommentContent { get; set; } // Assuming this is not editable
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; } // Assuming this is not editable
    }
}
