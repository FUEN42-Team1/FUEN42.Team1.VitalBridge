using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentCategoryRepository
    {
        Task<IEnumerable<ContentCategory>> GetAllAsync();
        Task<ContentCategory?> GetByIdAsync(int id);
        Task AddAsync(ContentCategory contentCategory);
        Task UpdateAsync(ContentCategory contentCategory);
        Task DeleteAsync(int id);
        Task<IEnumerable<ContentCategory>> GetByParentIdAsync(int? parentId);
    }
}
