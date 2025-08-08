using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ContentArticleViewModel
    {
    }

    public class ContentArticleListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte[] CoverPic { get; set; } // Base64 encoded image
        public string CategoryName { get; set; }
        public string MemberName { get; set; }
        public string Status { get; set; } // efmodel is int , but view model is string for display purposes
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class ContentArticleCreateViewModel
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public int ContentCategoryId { get; set; }

        public IFormFile? CoverPic { get; set; } 
        public string Status { get; set; } // efmodel is int , but view model is string for display purposes

    }
    public enum ContentArticleStatus
    {
        Drafted = 0,
        Published = 1,
        Archived = 2
    }
}
