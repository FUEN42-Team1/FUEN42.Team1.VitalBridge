using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
    public class ContentArticleRepository : IContentArticleRepository
    {
        private readonly AppDbContext _context;

        public ContentArticleRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async Task AddAsync(Content content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            content.CreatedAt = DateTime.UtcNow;
            content.UpdatedAt = DateTime.UtcNow;
            await _context.Contents.AddAsync(content);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
           var content = await _context.Contents.FindAsync(id);
            if (content == null)
            {
                throw new KeyNotFoundException($"Content with ID {id} not found.");
            }
            try
            {
                _context.Contents.Remove(content);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle the exception, e.g., log it or rethrow it
                throw new Exception("An error occurred while deleting the content.", ex);
            }
        }

        public async Task<IEnumerable<Content>> GetAllAsync()
        {
            return await _context.Contents
                .AsNoTracking()
                .ToListAsync();
        }

        public IQueryable<Content> GetAllWithIncludes()
        {
            return _context.Contents
                .Include(c => c.ContentCategory)
                .Include(c => c.Comments)
                //.Include(c => c.Member) // Assuming Member is a navigation property in Content
                .AsNoTracking();
        }

        public async Task<IEnumerable<Content>> GetByCategoryAsync(string category)
        {
            return await _context.Contents
                .Where(c => c.ContentCategory.Name == category)
                .Include(c => c.ContentCategory)
                .ToListAsync();
        }

        public async Task<Content?> GetByIdAsync(int id)
        {
            var content = await _context.Contents
                .Include(c => c.ContentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            return content;
        }

        public async Task<Content?> GetByNameAsync(string name)
        {
            var content = await _context.Contents
                .Include(c => c.ContentCategory)
                .FirstOrDefaultAsync(m => m.Title == name);
            return content;
        }

        public async Task UpdateAsync(Content content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            var existingContent = await _context.Contents.FindAsync(content.Id);
            if (existingContent == null)
            {
                throw new KeyNotFoundException($"Content with ID {content.Id} not found.");
            }
            existingContent.Title = content.Title;
            existingContent.CoverPic = content.CoverPic;
            existingContent.ContentCategoryId = content.ContentCategoryId;
            existingContent.Content1= content.Content1;
            existingContent.Status = content.Status;
            existingContent.ViewCount = content.ViewCount;
            existingContent.UpdatedAt = DateTime.UtcNow;

            //existingContent.DisplayOrder = content.DisplayOrder; 
            _context.Contents.Update(existingContent);
            await _context.SaveChangesAsync();
        }
    }
}
