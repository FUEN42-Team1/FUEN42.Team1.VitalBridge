using System.ComponentModel.DataAnnotations;
using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class ContentArticleViewModel
    {
    }

    public class ContentArticleListViewModel
    {
        [Display (Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "文章名稱")]
        public string Title { get; set; }

        public byte[] CoverPic { get; set; } // Base64 encoded image

        [Display(Name = "文章分類")]
        public string CategoryName { get; set; }
        
        [Display(Name = "作者")]
        public string MemberName { get; set; }

        [Display(Name = "狀態")]
        public string Status { get; set; } // efmodel is int , but view model is string for display purposes

        [Display(Name = "點閱數")]
        public int ViewCount { get; set; }

        [Display(Name = "建立時間")]
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class ContentArticleCreateViewModel
    {
        [Required]
        [Display(Name = "文章名稱")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "文章內容")]
        public string Content { get; set; }

        [Display(Name = "文章分類")]
        [Required]
        public int ContentCategoryId { get; set; }

        public IFormFile? CoverPic { get; set; }
        public string CoverPicFileName { get; set; }
        public string Status { get; set; } // efmodel is int , but view model is string for display purposes

    }
    public class ContentArticleEditViewModel
    {

        public int Id { get; set; }

        [Required]
        [Display(Name = "文章名稱")]
        public string Title { get; set; }
        [Required]
        [Display(Name = "文章內容")]
        public string Content { get; set; }
        [Display(Name = "文章分類")]
        [Required]
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
