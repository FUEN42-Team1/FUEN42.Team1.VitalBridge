using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentArticleRepository
    {
        Task<IEnumerable<Content>> GetAllAsync();
        public IQueryable<Content> GetAllWithIncludes();
        Task<IEnumerable<Content>> GetByCategoryAsync(string category);
        Task<Content?> GetByIdAsync(int id);
        Task<Content?> GetByNameAsync(string name);
        Task AddAsync(Content content);
        Task UpdateAsync(Content content);
        Task DeleteAsync(int id);
    }
}
