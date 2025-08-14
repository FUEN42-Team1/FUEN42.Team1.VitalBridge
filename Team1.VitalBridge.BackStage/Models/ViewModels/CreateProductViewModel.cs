using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class CreateProductViewModel
    {
        
        [Display(Name = "商品貨號")]
        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(100)]
        public string ItemNumber { get; set; }

        [Display(Name = "品名")]
        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "商品簡述")]
        [Required(ErrorMessage = "請輸入{0}")]
        [StringLength(250)]
        public string Keypoint { get; set; }

        [Display(Name = "商品描述")]
        [Required(ErrorMessage = "請輸入{0}")]
        public string ProductDescription { get; set; }

        [Display(Name = "價格")]
        [Required(ErrorMessage = "請輸入{0}")]
        public decimal Price { get; set; }

        [Display(Name = "庫存")]
        [Required(ErrorMessage = "請輸入{0}")]
        public int Quantity { get; set; } = 0;

        [Display(Name = "啟用狀態")]
        [Required(ErrorMessage = "請選擇{0}")]
        public bool IsActive { get; set; }=false;

        // 這邊要儲存圖片
        public string? Image1FileName { get; set; }
        public string? Image2FileName { get; set; }
        public string? Image3FileName { get; set; }
        public string? Image4FileName { get; set; }
        public string? Image5FileName { get; set; }
        public string? Image6FileName { get; set; }
        public string? Image7FileName { get; set; }
        public string? Image8FileName { get; set; }

        // 處理選類別
        // 選中的類別ID
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
        // 給前端顯示的類別選項
        public List<CategorySelectionItemViewModel> Categories { get; set; } = new List<CategorySelectionItemViewModel>();

    }
}
