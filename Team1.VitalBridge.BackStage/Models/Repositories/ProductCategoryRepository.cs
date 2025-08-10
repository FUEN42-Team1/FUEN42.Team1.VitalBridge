using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
	public class ProductCategoryRepository : IProductCategoryRepository
	{
		private readonly AppDbContext _context;

		public ProductCategoryRepository(AppDbContext contexxt)
		{
			this._context = contexxt;
		}
		public async Task<List<Category>> GetAllAsync()
		{
			return await _context.Categories
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		
		// 根據ID取得類別
		public async Task<Category> GetByIdAsync(int id)
		{
			// 使用 FindAsync 方法查找指定 ID 的類別
			return await _context.Categories.FindAsync(id);
		}

		// 新增商品類別資料
		public async Task<Category> CreateAsync(Category category)
		{
			
			_context.Categories.Add(category);
			await _context.SaveChangesAsync();
			return category;
		}

		// 檢查類別是否存在
		public Task<bool> ExistsAsync(int id)
		{
			// 使用 AnyAsync 方法檢查是否有任何類別的 ID 匹配
			return _context.Categories.AnyAsync(c => c.Id == id);
		}

		// 取得啟用的類別(用於父類別選項)
		public async Task<List<Category>> GetActiveParentOptionAsync()
		{
			// 取得所有啟用的父類別（FatherId 為 null 的類別）
			return await _context.Categories
				.Where(c => c.IsActive && c.FatherId == null)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		// 取得子類別
		public async Task<List<Category>> GetChildrenAsync(int fatherId)
		{
			// 取得指定父類別的所有子類別
			return await _context.Categories
				.Where(c => c.FatherId == fatherId)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		// 檢查類別名稱是否已存在 (新增使用)
		public Task<bool> IsNameExistsAsync(string name)
		{
			// 使用 AnyAsync 方法檢查是否有任何類別的名稱匹配
			// 如果名稱已存在，則返回 true
			return _context.Categories
				.AnyAsync(c => c.Name == name);
		}

		// 檢查類別名稱是否已存在 (更新使用，排除自己) -- 待補
		public Task<bool> IsNameExistsAsync(string name, int excludeId)
		{
			throw new NotImplementedException();
		}

		public Task<Category> UpdateAsync(Category category)
		{
			throw new NotImplementedException();
		}

		public Task<bool> DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}
	}
}
