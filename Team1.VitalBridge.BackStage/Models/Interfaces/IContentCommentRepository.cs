using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentCommentRepository
    {
        // Define methods for managing content comments
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task AddAsync(Comment contentComment);
        Task UpdateAsync(Comment contentComment);
        Task DeleteAsync(int id);
        Task<IEnumerable<Comment>> GetByArticleIdAsync(int articleId);
    }
}
