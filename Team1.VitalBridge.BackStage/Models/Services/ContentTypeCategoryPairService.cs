using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.Repositories;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class ContentTypeCategoryPairService : IContentTypeCategoryPairService
    {
        private readonly IContentTypeCategoryPairRepository _repositary;

        public ContentTypeCategoryPairService(IContentTypeCategoryPairRepository repositary)
        {
            this._repositary = repositary;
        }

        public void Create(ContentTypeCategoryPairCreateDTO contentTypeCategoryPairCreateDTO)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentTypeCategoryPairDisplayDTO> GetAll()
        {
            var entity = _repositary.GetAll()
                .OrderBy(pair => pair.ContentType.DisplayOrder)
                .ThenBy(pair => pair.ContentCategory.ParentCategory != null
                     ? pair.ContentCategory.ParentCategory.DisplayOrder
                     : int.MinValue)
                .ThenBy(pair => pair.ContentCategory.DisplayOrder);

            return entity.Select(pair => new ContentTypeCategoryPairDisplayDTO
            {
                Id = pair.Id,
                ContentTypeId = pair.ContentTypeId,
                ContentTypeName = pair.ContentType.Name,
                ContentCategoryDisplayOrder = pair.ContentCategory.DisplayOrder,

                ParentContentCategoryName = pair.ContentCategory.ParentCategoryId==null
                ?null
                :pair.ContentCategory.ParentCategory.Name,
                ParentContentCategoryDisplayOrder = pair.ContentCategory.ParentCategoryId == null
                ? null
                :pair.ContentCategory.ParentCategory.DisplayOrder,

                ContentCategoryId = pair.ContentCategoryId,
                ContentCategoryName = pair.ContentCategory.Name,
                ContentTypeDisplayOrder = pair.ContentType.DisplayOrder,
                IsEnabled = pair.IsEnabled
            }).ToList();
        }

        public IEnumerable<ContentTypeCategoryPairDisplayDTO> GetByCategoryId(int categoryId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentTypeCategoryPairDisplayDTO> GetByContentTypeId(int contentTypeId)
        {
            throw new NotImplementedException();
        }

        public ContentTypeCategoryPairDisplayDTO? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public ContentTypeCategoryPairDisplayDTO? GetByName(string name)
        {
            throw new NotImplementedException();
        }

        public void Update(ContentTypeCategoryPairEditDTO contentTypeCategoryPairEditDTO)
        {
            throw new NotImplementedException();
        }
    }
}
