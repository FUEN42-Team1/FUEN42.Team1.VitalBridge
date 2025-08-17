using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class OrderListItemViewModel
	{
		// 訂單Id
		public int Id { get; set; }

		[Display(Name = "訂單編號")]
		public string OrderNumber { get; set; }

		// 成立日期
		[Display(Name = "成立日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd HH:mm}")]
		public DateTime CreatedAt { get; set; }



		[Display(Name = "會員名稱")]
		public string CustomerName { get; set; }

		[Display(Name = "付款狀態")]
		public string  PaymentStatus { get; set; }

		[Display(Name = "物流方式")]
		public string ShippingMethod { get; set; }


		/// <summary>
		/// 當前訂單狀態ID
		/// </summary>
		public int CurrentStatusId { get; set; }

		/// <summary>
		/// 當前訂單狀態名稱
		/// </summary>
		[Display(Name = "訂單狀態")]
		public string CurrentStatusName { get; set; }

		/// <summary>
		/// 訂單總金額
		/// </summary>
		[Display(Name = "訂單總金額")]
		[DisplayFormat(DataFormatString = "{0:N0}")]
		public decimal TotalAmount { get; set; }

		
	}
}