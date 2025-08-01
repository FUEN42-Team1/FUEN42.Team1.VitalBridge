using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentTypeCategoryPairService
    {
        public void Create(ContentTypeCategoryPairCreateDTO contentTypeCategoryPairCreateDTO);
        public void Delete(int id);
        public IEnumerable<ContentTypeCategoryPairDisplayDTO> GetAll();
        public IEnumerable<ContentTypeCategoryPairDisplayDTO> GetByCategoryId(int categoryId);
        public IEnumerable<ContentTypeCategoryPairDisplayDTO> GetByContentTypeId(int contentTypeId);
        public ContentTypeCategoryPairDisplayDTO? GetById(int id);
        public ContentTypeCategoryPairDisplayDTO? GetByName(string name);
        public void Update(ContentTypeCategoryPairEditDTO contentTypeCategoryPairEditDTO);
    }
}
