using Microsoft.Identity.Client;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class ProductCategoryService
    {
		// 商品類別業務邏輯

		private readonly IProductCategoryRepository _repository;

		
		public ProductCategoryService(IProductCategoryRepository repository)
		{
			this._repository = repository;
		}

		// 取得類別樹狀列表
		public async Task<List<ProductCategoryDto>> GetCategoryTreeAsync()
		{
			// 1. 取得所有類別資料
			var categories = await _repository.GetAllAsync();


			// 建立一個清單來儲存樹狀結構的類別資料容器

			var categoryTreelist = new List<ProductCategoryDto>();

			// 3. 先處理第一層（ParentId == null）
			var rootCategories = categories
				.Where(c => c.FatherId == null).ToList();
			foreach (var root in rootCategories)
			{
				categoryTreelist.Add(new ProductCategoryDto
				{
					Id = root.Id,
					Name = root.Name,
					FatherId = root.FatherId,
					IsActive = root.IsActive,
					Level = 0

				});

				// 4. 處理子類別
				// 在categories 裡面 篩選出 FatherId 已經有填的，也就是上面的root.Id
				// 因為root.id 代表已經是父類別，這邊就是在找所有root的子類別
				var children = categories.Where(c => c.FatherId == root.Id).ToList();
				foreach (var child in children)
				{
					categoryTreelist.Add(new ProductCategoryDto
					{
						Id = child.Id,
						Name = child.Name,
						FatherId = child.FatherId,
						IsActive = child.IsActive,
						Level = 1,
						FatherName = root.Name // 設定父類別名稱

					});
				}
			}
			return categoryTreelist;

		}


	}
}
