using System.ComponentModel.DataAnnotations;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
	public class ProductListViewModel
	{
		public int Id { get; set; }

		[Display(Name = "品名")]
		public string Name { get; set; }

		[Display(Name = "商品貨號")]
		public string ItemNumber { get; set; }


		[Display(Name = "價格")]
		public decimal Price { get; set; }

		[Display(Name = "庫存")]
		public int Quantity { get; set; }

		[Display(Name = "上架狀態")]
		public bool IsActive { get; set; }
	}
	// 寫擴充方法
	public static class ProductExtensions
	{

		public static ProductListViewModel ToIndexVm(this Product p)
		{
			return new ProductListViewModel
			{
				Id = p.Id,
				Name = p.Name,
				ItemNumber = p.ItemNumber,
				Price = p.Price,
				Quantity = p.Quantity,
				IsActive = p.IsActive

			};

		}



	}


}
