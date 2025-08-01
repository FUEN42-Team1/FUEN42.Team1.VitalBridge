using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductCategoryFormViewModel
	{

		public int? Id { get; set; } // 新增還沒存入資料的時候，為null，在Service做判斷。
		public int? FatherId { get; set; }

		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		public string BannerImageUrl { get; set; }

		public bool IsActive { get; set; } 
	}
}
