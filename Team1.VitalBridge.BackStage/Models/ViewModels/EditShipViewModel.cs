using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class EditShipViewModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "請輸入運送方式名稱")]
		[StringLength(50)]
		[Display(Name = "運送方式名稱")]
		public string ShipMethodName { get; set; }

		[Required(ErrorMessage = "請輸入運費")]
		[Range(0, double.MaxValue, ErrorMessage = "運費必須是正數")]
		[DataType(DataType.Currency)]
		[Display(Name = "運費")]
		public decimal ShipCost { get; set; }

		[Display(Name = "啟用狀態")]
		public bool IsActive { get; set; } 
	}
}
