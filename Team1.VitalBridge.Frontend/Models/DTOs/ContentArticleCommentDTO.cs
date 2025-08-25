namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    public class ContentArticleCommentDTO
    {
        public string author { get; set; }
        public string handle { get; set; }
        public string time { get; set; }
        public string text { get; set; }
    }
    public class  ArticleCommentRequestDTO
    {
        public int ArticleId { get; set; }
        public int Page { get; set; } // Page number for pagination
        public int PageSize { get; set; } // Number of comments per page

        public int? ParentCommentId { get; set; } // Parent Comment ID to filter comments

    }
}
