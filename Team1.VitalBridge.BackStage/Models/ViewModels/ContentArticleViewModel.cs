namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ContentArticleViewModel
    {
    }

    public class ContentArticleListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public byte[] CoverPic { get; set; }
        public int ContentCategoryId { get; set; }
        public string  ContentCategoryName { get; set; } 
        public string Status { get; set; } // efmodel is int , but view model is string for display purposes
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
