using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
    public class ContentCommentRepository : IContentCommentRepository
    {
        private readonly AppDbContext _context;

        public ContentCommentRepository(AppDbContext context)
        {
            this._context = context;
        }
        public async Task AddAsync(Comment contentComment)
        {
            if (contentComment == null)
            {
                throw new ArgumentNullException(nameof(contentComment), "Content comment cannot be null");
            }
            _context.Comments.Add(contentComment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                throw new KeyNotFoundException($"Comment with ID {id} not found.");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            var comments = await _context.Comments.ToListAsync();
            return comments;
        }

        public async Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId)
        {
            if (articleId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(articleId), "Article ID must be greater than zero.");
            }
            var comments = await _context.Comments
                .Where(c => c.ContentId == articleId)
                .ToListAsync();
            return comments;
        }

        public async Task<Comment?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "ID must be greater than zero.");
            }
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                throw new KeyNotFoundException($"Comment with ID {id} not found.");
            }
            return comment;
        }

        public async Task UpdateAsync(Comment contentComment)
        {
            if (contentComment == null)
            {
                throw new ArgumentNullException(nameof(contentComment), "Content comment cannot be null");
            }
            var existingComment = await _context.Comments.FindAsync(contentComment.Id);
            if (existingComment == null)
            {
                throw new KeyNotFoundException($"Comment with ID {contentComment.Id} not found.");
            }
            _context.Entry(existingComment).CurrentValues.SetValues(contentComment);
            await _context.SaveChangesAsync();
        }
    }
}
