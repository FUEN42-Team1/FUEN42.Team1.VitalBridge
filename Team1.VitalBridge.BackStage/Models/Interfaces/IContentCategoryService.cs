using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentCategoryService
    {
        public Task<IEnumerable<ContentCategoryDTO>> GetAllCategoriesAsync();
        public Task<IEnumerable<ContentCategoryDTO?>> GetSubCategoriesAsync(int? parentId);
        public Task<ContentCategoryDTO> GetCategoryByIdAsync(int id);
        public Task AddCategoryAsync(ContentCategoryCreateDTO category);
        public Task UpdateCategoryAsync(ContentCategoryEditDTO category);
        public Task DeleteCategoryAsync(int id);
    }
}
