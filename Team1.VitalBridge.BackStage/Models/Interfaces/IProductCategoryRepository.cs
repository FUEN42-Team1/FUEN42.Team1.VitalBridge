
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
	public interface IProductCategoryRepository
	{
		// 取得所有商品類別
		Task<List<Category>> GetAllAsync();

		// 根據ID取商品類別資料
		Task<Category> GetByIdAsync(int id);

		// 新增商品類別資料
		Task <Category>CreateAsync(Category category);

		// 更新商品類別資料
		Task<Category> UpdateAsync(Category category);

		// 刪除商品類別資料 ，boolean表示是否成功
		Task<bool> DeleteAsync(int id);

		// 取得啟用的類別(用於父類別選項)

		Task<List<Category>> GetActiveParentOptionAsync();

		// 取得指定類別的子類別
		Task<List<Category>> GetChildrenAsync(int fatherId);

		//檢查類別是否存在
		Task<bool> ExistsAsync(int id);

		// 檢查類別名稱是否已存在 (新增使用)
		Task<bool> IsNameExistsAsync(string name);

		// 檢查類別名稱是否已存在 (更新使用，排除自己)
		Task<bool> IsNameExistsAsync(string name, int excludeId);

        // 從FileStream中的FileName取得FileId
		Task<int?> GetFileIdByFileNameAsync(string fileName);




	}
}
