using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class ContentCategoryService : IContentCategoryService
    {
        private readonly IContentCategoryRepository _repository;

        public ContentCategoryService(IContentCategoryRepository repository)
        {
            this._repository = repository;
        }

        public async Task AddCategoryAsync(ContentCategoryDTO category)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ContentCategoryDTO>> GetAllCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ContentCategoryDTO?>> GetSubCategoriesAsync(int? parentId)
        {
            var entities = await _repository.GetByParentIdAsync(parentId);
            if (entities == null || !entities.Any())
            {
                return Enumerable.Empty<ContentCategoryDTO>();
            }

            // Convert EF models to DTOs
            var dtoList = entities.Select(c => new ContentCategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                IsEnabled = c.IsEnabled,
                DisplayOrder = c.DisplayOrder,
                UpdatedAt = c.UpdatedAt,
                CreatedAt = c.CreatedAt
            }).ToList();

            return dtoList;
        }

        public async Task<ContentCategoryDTO> GetCategoryByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateCategoryAsync(ContentCategoryDTO category)
        {
            throw new NotImplementedException();
        }
    }
}
