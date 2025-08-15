using System.Reflection.Metadata;

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

    public class ContentCommentListViewModel_I
    {
        public int Id { get; set; }
        //public string MemberName { get; set; }
        public string ContentTitle { get; set; }
        public int? ParentCommentId { get; set; }
        public string Content { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ArticleCommentListViewModel
    {
        public int Id { get; set; } // Hidden in the view
        
        public string ArticleTitle { get; set; } // Assuming this is not editable
        public int ArticleId { get; set; } // Assuming this is hidden in the view
        public string MemberName { get; set; } // Assuming this is not editable
        public int MemberId { get; set; } // Assuming this is hidden in the view
        public int? ParentCommentId { get; set; } // Assuming this is not editable
        public string Content { get; set; } // Assuming this is not editable
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; } // Assuming this is not editable
    }

    public class ContentCommentEditViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public int? ParentCommentId { get; set; }
        public bool IsPinned { get; set; }
        public DateTime CreatedAt { get; set; }
        //public string MemberName { get; set; } // Assuming this is not editable
        public string ContentTitle { get; set; } // Assuming this is not editable
        public int ContentId { get; set; } // Assuming this is the ID of the content the comment belongs to
    }
}
