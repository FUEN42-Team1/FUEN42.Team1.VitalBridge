namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class ContentArticleDTO
    {
    }

    public class ContentArticleListDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte[]? CoverPic { get; set; } // Base64 encoded image

        public int ContentCategoryId { get; set; }
        public string CategoryName { get; set; }

        public int MemberId { get; set; }
        public string MemberName { get; set; }

        public int Status { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class ContentArticleListCritriaDTO
    {
        /// <summary>
        /// Keyword search for Title, Category Name, or Member Name
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// Article status (for select dropdown)
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Minimum view count
        /// </summary>
        public int? MinViewCount { get; set; }

        /// <summary>
        /// Maximum view count
        /// </summary>
        public int? MaxViewCount { get; set; }

        /// <summary>
        /// Start date for creation time
        /// </summary>
        public DateTime? CreatedAtStart { get; set; }

        /// <summary>
        /// End date for creation time
        /// </summary>
        public DateTime? CreatedAtEnd { get; set; }

        /// <summary>
        /// Start date for last updated time
        /// </summary>
        public DateTime? UpdatedAtStart { get; set; }

        /// <summary>
        /// End date for last updated time
        /// </summary>
        public DateTime? UpdatedAtEnd { get; set; }
    }

    public class ContentArticleCreateDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int MemberId { get; set; } // MemberId of the author
        public int ContentCategoryId { get; set; }
        public IFormFile? CoverPic { get; set; } // Base64 encoded image
        public int Status { get; set; } // efmodel is int , but view model is string for display purposes
    }
    public class ContentArticleEditDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int ContentCategoryId { get; set; }
        public IFormFile? CoverPic { get; set; } // Base64 encoded image
        public int Status { get; set; } // efmodel is int , but view model is string for display purposes
    }
}
