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
		/// <summary>
		/// 主圖 URL（新增）
		/// </summary>
		[Display(Name = "主圖")]
		public string MainImageUrl { get; set; } = "";
	}
	// 寫擴充方法
	public static class ProductExtensions
	{

		public static ProductListViewModel ToIndexVm(this Product p)
		{
			var viewModel = new ProductListViewModel
			{
				Id = p.Id,
				Name = p.Name,
				ItemNumber = p.ItemNumber,
				Price = p.Price,
				Quantity = p.Quantity,
				IsActive = p.IsActive
			};

			// 尋找主圖（SortOrder = 1）
			var mainImage = p.ProductImages
				?.Where(pi => pi.SortOrder == 1 && pi.File != null && !string.IsNullOrEmpty(pi.File.FileName))
				.FirstOrDefault();

			// 如果有主圖，生成圖片 URL
			if (mainImage != null)
			{
				viewModel.MainImageUrl = $"/api/UploadFile/GetFile?fileName={Uri.EscapeDataString(mainImage.File.FileName)}";
			}

			return viewModel;
		}



	}


}
