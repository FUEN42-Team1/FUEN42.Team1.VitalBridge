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

		public Task<Category> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<Category> CreateAsync(Category category)
		{
			throw new NotImplementedException();
		}

		public Task<bool> DeleteAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<bool> ExistsAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<List<Category>> GetActiveParentOptionAsync()
		{
			throw new NotImplementedException();
		}

		

		public Task<List<Category>> GetChildrenAsync(int fatherId)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsNameExistsAsync(string name)
		{
			throw new NotImplementedException();
		}

		public Task<bool> IsNameExistsAsync(string name, int excludeId)
		{
			throw new NotImplementedException();
		}

		public Task<Category> UpdateAsync(Category category)
		{
			throw new NotImplementedException();
		}
	}
}
