namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    public class ContentArticleDTO
    {
    }
    public class ContentArticleViewPlusOneDTO
    {
        public int articleId { get; set; }
    }
    public class ArticleRequestDTO
    {
        public int Page { get; set; } // Page number for pagination 
        public int PageSize { get; set; } // Number of articles per page
        public int? CategoryId { get; set; } // Category ID to filter articles
    }
    public class ContentArticleDisplayDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? CoverPic { get; set; } // Base64 image data
        public string? CoverPicFileName { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public string Excerpt { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
    }

    public class ContentArticleContentDisplayDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? CoverPicFileName { get; set; }
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
    }



}
