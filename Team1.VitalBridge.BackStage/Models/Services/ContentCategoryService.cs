using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
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

        public async Task AddCategoryAsync(ContentCategoryCreateDTO category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "Category cannot be null");
            }
            // Convert DTO to EF model
            var entity = new ContentCategory
            {
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                IsEnabled = category.IsEnabled,
                DisplayOrder = category.DisplayOrder
                //CreatedAt = DateTime.Now,
                //UpdatedAt = DateTime.Now
            };
            await _repository.AddAsync(entity);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _repository.DeleteAsync(id);
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
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"ContentCategory with ID {id} not found.");
            }
            // Convert EF model to DTO
            var dto = new ContentCategoryDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentCategoryId = entity.ParentCategoryId,
                IsEnabled = entity.IsEnabled,
                DisplayOrder = entity.DisplayOrder,
                UpdatedAt = entity.UpdatedAt,
                CreatedAt = entity.CreatedAt
            };
            return dto;
        }

        public async Task UpdateCategoryAsync(ContentCategoryEditDTO category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category), "Category cannot be null");
            }
            // Convert DTO to EF model
            var entity = new ContentCategory
            {
                Id = category.Id,
                Name = category.Name,
                ParentCategoryId = category.ParentCategoryId,
                IsEnabled = category.IsEnabled,
                DisplayOrder = category.DisplayOrder,
                //UpdatedAt = DateTime.UtcNow // Update timestamp
            };
            await _repository.UpdateAsync(entity);

        }
    }
}
