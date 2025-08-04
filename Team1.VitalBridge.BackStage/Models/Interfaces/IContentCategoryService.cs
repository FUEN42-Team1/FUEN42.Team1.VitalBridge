using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentCategoryService
    {
        public Task<IEnumerable<ContentCategoryDTO>> GetAllCategoriesAsync();
        public Task<IEnumerable<ContentCategoryDTO?>> GetSubCategoriesAsync(int? parentId);
        public Task<ContentCategoryDTO> GetCategoryByIdAsync(int id);
        public Task AddCategoryAsync(ContentCategoryDTO category);
        public Task UpdateCategoryAsync(ContentCategoryDTO category);
        public Task DeleteCategoryAsync(int id);
    }
}
