using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class OrderListViewModel
	{
		/// <summary>
		/// 這一份是主要的訂單列表資料模型 (主容器)
		/// </summary>


		// 訂單列表資料
		public List<OrderListItemViewModel> Orders { get; set; } = new List<OrderListItemViewModel>();

		// 分頁資訊
		public PaginationViewModel Pagination { get; set; } = new PaginationViewModel();

		// 訂單狀態選項(例如：待付款、待出貨、已出貨、已完成、退貨中、已退貨、已取消等)
		public List<OrderStatusOptionViewModel> StatusOptions { get; set; } = new List<OrderStatusOptionViewModel>();

		// 篩選條件
		 public OrderSearchViewModel Search { get; set; } = new OrderSearchViewModel();



	}
}
