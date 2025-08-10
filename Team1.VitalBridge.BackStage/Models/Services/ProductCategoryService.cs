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


        // 新增商品類別
		public async Task<ProductCategoryDto> CreateAsync(CreateProductCategoryDto createDto)
		{
			// 1. 手動驗證輸入資料
			await ValidateCreateDtoAsync(createDto);

            // 驗證階層規則
            var validationError = await ValidateHierarchyAsync(null, createDto.FatherId);
            if (!string.IsNullOrEmpty(validationError))
            {
                throw new InvalidOperationException(validationError);
            }



            // 2. 將 DTO 轉換為 EF 模型
            var newCategory = new Category
			{
				Name = createDto.Name,
				FatherId = createDto.FatherId,
				IsActive = createDto.IsActive
			};
			// 3. 呼叫儲存庫方法新增類別
			var createdCategory = await _repository.CreateAsync(newCategory);
			// 4. 將 EF 模型轉換為 DTO 並返回
			return new ProductCategoryDto
			{
				Id = createdCategory.Id,
				Name = createdCategory.Name,
				FatherId = createdCategory.FatherId,
				IsActive = createdCategory.IsActive,
				Level = createdCategory.FatherId == null ? 0 : 1 // 根據是否有父類別設定層級
			};
        }


        // 驗證階層規則
        private async Task<string> ValidateHierarchyAsync(int? categoryId, int? fatherId)
        {
            // HasValue 是可為 null 的數值型別，用來判斷這個變數目前有沒有實際的值
            // 如果父類別ID為空，則不需要進行階層驗證
            if (!fatherId.HasValue) return "";

            // 不能選擇自己作為父類別
			


        }

        private async Task ValidateCreateDtoAsync(CreateProductCategoryDto createDto)
        {
            var errors = new List<string>();

            // 檢查名稱
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                errors.Add("名稱不能為空白");
            }
            else if (createDto.Name.Length > 50)
            {
                errors.Add("名稱不能超過50個字元");
            }

            // 檢查名稱是否重複 -- 用
            // string.IsNullOrWhiteSpace 靜態方法檢查一個字串是否，是 null、空字串、包含空白字元
            if (!string.IsNullOrWhiteSpace(createDto.Name))
            {
                if (await _repository.IsNameExistsAsync(createDto.Name))
                {
                    errors.Add($"名稱 '{createDto.Name}' 已經存在");
                }

            }

            if (errors.Any())
            {
                throw new ArgumentException(string.Join(", ", errors));
            }
        }

    }
}
