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


		// 根據ID取得單一類別
		public async Task<ProductCategoryDto> GetByIdAsync(int id) { 
		
		
			var category = await _repository.GetByIdAsync(id);
			if (category == null) return null;

            //取得父類別
            string? fatherName = null;
            if (category.FatherId.HasValue)
            {
                var parent = await _repository.GetByIdAsync(category.FatherId.Value);
                fatherName = parent?.Name;
            }

            // 將 EF 模型轉換為 DTO
			return new ProductCategoryDto
			{
				Id = category.Id,
				Name = category.Name,
				FatherId = category.FatherId,
				IsActive = category.IsActive,
				Level = category.FatherId == null ? 0 : 1, // 根據是否有父類別設定層級
				FatherName = fatherName // 設定父類別名稱
			};



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

            // 2. 將 DTO 的資料轉換為 EF 模型
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
			if (categoryId.HasValue && categoryId.Value == fatherId.Value)
			{
				return "不能將自己設為父類別";
			}

			// 檢查父類別是否存在 用儲存庫方法
			var parent = await _repository.GetByIdAsync(fatherId.Value);
			// 如果父類別不存在，則返回錯誤訊息
			if (parent == null)
			{
				return "選擇的父類別不存在";
			}


			// 檢查父類別是否已經是第二層 (不允許3層)
			//HasValue 是可為 null 的數值型別，用來判斷這個變數目前有沒有實際的值
			if (parent.FatherId.HasValue)
			{
				// parent 在上面宣告已經有賦值
				// 判斷parent.FatherId.HasValue 是否實的值，有的話進入這裡
				// 如果父類別已經有父類別，則表示這個父類別已經是第二層
				return "不允許選擇第二層以上的父類別";
			}


			// 如果是編輯，檢查是否選擇了自己的子類別作為父類別 (等寫編輯再來寫)



			return ""; // 驗證通過

		}


		// 驗證新增商品類別的 DTO
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


			// 檢查名稱是否重複  使用 Repository 的IsNameExistsAsync方法 為True的時候會進行
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

		// 取得父類別選項
		public async Task<List<ProductCategoryDto>> GetParentOptionsAsync()
		{
			// 1. 從資料庫查詢所有啟用的類別 
			var parentCategories = await _repository.GetActiveParentOptionAsync();

			var result = new List<ProductCategoryDto>();

			// 2. 只取根類別作為父類別選項 (因為只允許2層)
			var rootCategories = parentCategories
				.Where(c => c.FatherId == null);


			// 3. 不再過濾，回傳所有根類別選項
			// 前端會自行判斷哪些要 disable
			foreach (var category in rootCategories)
			{
				// 將每個根類別轉換為 DTO
				result.Add(new ProductCategoryDto
				{
					Id = category.Id,
					Name = category.Name,
					FatherId = category.FatherId,
					IsActive = category.IsActive,
					Level = 0 // 根類別層級為0
				});
			}

			
			return result;


		}


        // 更新商品類別
		public async Task<ProductCategoryDto> UpdateAsync(UpdateProductCategoryDto updateDto)
		{
			// 1. 驗證輸入資料
			await ValidateUpdateDtoAsync(updateDto);
			// 2. 檢查階層規則
			var validationError = await ValidateHierarchyAsync(updateDto.Id, updateDto.FatherId);
			if (!string.IsNullOrEmpty(validationError))
			{
				throw new InvalidOperationException(validationError);
			}
			// 3. 取得現有類別資料
			var category = await _repository.GetByIdAsync(updateDto.Id);
			if (category == null)
			{
                //KeyNotFoundException 是從字典或集合中取得不存在的鍵時拋出的異常
                throw new KeyNotFoundException($"類別 ID {updateDto.Id} 不存在");
			}
			// 4. 更新類別資料
			category.Name = updateDto.Name;
			category.FatherId = updateDto.FatherId;
			category.IsActive = updateDto.IsActive;
			// 5. 呼叫儲存庫方法更新類別
			var updateCatefory =  await _repository.UpdateAsync(category);

            // 6. 將 EF 模型轉換為 DTO 並返回
            return new ProductCategoryDto
            {
                Id = updateCatefory.Id,
                Name = updateCatefory.Name,
                FatherId = updateCatefory.FatherId,
                IsActive = updateCatefory.IsActive,
                Level = updateCatefory.FatherId == null ? 0 : 1 // 根據是否有父類別設定層級
            };


        }


        // 檢查更新商品類別的 DTO的驗證
        private async Task ValidateUpdateDtoAsync(UpdateProductCategoryDto updateDto)
        {
            var errors = new List<string>();

            // 檢查 ID
            if (updateDto.Id <= 0)
            {
                errors.Add("類別ID無效");
            }

            // 檢查名稱
            if (string.IsNullOrWhiteSpace(updateDto.Name))
            {
                errors.Add("類別名稱為必填欄位");
            }
            else if (updateDto.Name.Length > 50)
            {
                errors.Add("類別名稱不能超過50個字元");
            }

            // 檢查名稱是否重複 - 使用 Repository 的方法 (排除自己)
            if (!string.IsNullOrWhiteSpace(updateDto.Name))
            {
                if (await _repository.IsNameExistsAsync(updateDto.Name, updateDto.Id))
                {
                    errors.Add("類別名稱已存在");
                }
            }

            if (errors.Any())
            {
                throw new ArgumentException(string.Join("; ", errors));
            }

        }
    }
}
