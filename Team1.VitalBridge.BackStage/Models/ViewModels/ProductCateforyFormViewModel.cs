using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductCateforyFormViewModel
	{
		// 這是用來新增、修改 類別
		
		public int? FatherId { get; set; }

		[Required]
		public string name { get; set; }
		public bool IsActive { get; set; } = true; // 預設開啟
		public int Level { get; set; } // 用於縮排顯示

		public List<SelectListItem>? ParentOptions { get; set; }  // 用於下拉式選單


	}
}
