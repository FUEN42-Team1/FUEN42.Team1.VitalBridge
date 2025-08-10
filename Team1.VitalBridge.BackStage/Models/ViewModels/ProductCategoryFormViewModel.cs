using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductCategoryFormViewModel
	{
		// 這是用來新增、修改 類別

		public int? Id { get; set; }

        [Display(Name = "上層類別")]
        public int? FatherId { get; set; }

		[Required]
        [Display(Name = "類別名稱")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "啟用狀態")]
        public bool IsActive { get; set; } = true; // 預設開啟



        // 父類別選項 -由 JavaScript 動態載入
        // 實際使用時會是空的，由前端 JavaScript 呼叫 API 載入
        public List<SelectListItem>? ParentOptions { get; set; }  // 用於下拉式選單

		public ProductCategoryFormViewModel()
		{
			ParentOptions = new List<SelectListItem>();
		}

        

    }
}
