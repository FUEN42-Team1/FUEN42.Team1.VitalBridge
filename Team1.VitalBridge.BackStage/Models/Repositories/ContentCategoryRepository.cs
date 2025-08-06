using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
    public class ContentCategoryRepository : IContentCategoryRepository
    {
        private readonly AppDbContext _context;

        public ContentCategoryRepository(AppDbContext context)
        {
            this._context = context;
        }
        public async Task AddAsync(ContentCategory contentCategory)
        {
            if (contentCategory == null)
            {
                throw new ArgumentNullException(nameof(contentCategory));
            }
            contentCategory.CreatedAt = DateTime.UtcNow;
            contentCategory.UpdatedAt = DateTime.UtcNow;
            await _context.ContentCategories.AddAsync(contentCategory);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var contentCategory = await _context.ContentCategories.FindAsync(id);
            if (contentCategory == null)
            {
                throw new KeyNotFoundException($"ContentCategory with ID {id} not found.");
            }

            try
            {
                _context.ContentCategories.Remove(contentCategory);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle the exception, e.g., log it or rethrow it
                throw new Exception("An error occurred while deleting the content category.", ex);
            }
        }

        public async Task<IEnumerable<ContentCategory>> GetAllAsync()
        {
            return await _context.ContentCategories
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ContentCategory?> GetByIdAsync(int id)
        {
            var contentCategory = await _context.ContentCategories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            return contentCategory;
        }

        public async Task<IEnumerable<ContentCategory>> GetByParentIdAsync(int? parentId)
        {
            var contentCategories = _context.ContentCategories
                .AsNoTracking()
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new ContentCategory
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId,
                    IsEnabled = c.IsEnabled,
                    DisplayOrder = c.DisplayOrder,
                    UpdatedAt = c.UpdatedAt,
                    CreatedAt = c.CreatedAt
                })
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return await contentCategories;
        }

        public async Task UpdateAsync(ContentCategory contentCategory)
        {
            if (contentCategory == null)
            {
                throw new ArgumentNullException(nameof(contentCategory));
            }
            var existingCategory = await _context.ContentCategories.FindAsync(contentCategory.Id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"ContentCategory with ID {contentCategory.Id} not found.");
            }
            existingCategory.Name = contentCategory.Name;
            existingCategory.ParentCategoryId = contentCategory.ParentCategoryId;
            existingCategory.IsEnabled = contentCategory.IsEnabled;
            existingCategory.DisplayOrder = contentCategory.DisplayOrder;
            existingCategory.UpdatedAt = DateTime.UtcNow;
            _context.ContentCategories.Update(existingCategory);
            await _context.SaveChangesAsync();
        }
    }
}
