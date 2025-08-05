using System.ComponentModel.DataAnnotations.Schema;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductCategoryTreeViewModel
	{
		// 這是顯示樹狀圖類別的ViewModel
		
		public int Id { get; set; }
		public int? FatherId { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public int Level { get; set; } // 用於縮排顯示 

	}
}
