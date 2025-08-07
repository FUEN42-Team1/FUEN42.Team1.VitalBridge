using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductCategoryFormViewModel
	{
		// 這是用來新增、修改 類別

		public int? Id { get; set; }
		public int? FatherId { get; set; }

		[Required]
		public string Name { get; set; }

        [Required]
        public bool IsActive { get; set; } = true; // 預設開啟

        public int FileId { get; set; } // 圖片檔案ID

        public List<SelectListItem>? ParentOptions { get; set; }  // 用於下拉式選單

		public ProductCategoryFormViewModel()
		{
			ParentOptions = new List<SelectListItem>();
		}

        public class ProductCategoryTreeViewModel
        {
            // 這是顯示樹狀圖類別的ViewModel

            public int Id { get; set; }
            public int? FatherId { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; } = true;
            public int Level { get; set; } // 用於縮排顯示 

        }

    }
}
