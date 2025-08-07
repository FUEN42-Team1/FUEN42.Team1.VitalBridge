using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentArticleRepository
    {
        Task<IEnumerable<Content>> GetAllAsync();
        Task<Content?> GetByIdAsync(int id);
        Task AddAsync(Content content);
        Task UpdateAsync(Content content);
        Task DeleteAsync(int id);
    }
}
