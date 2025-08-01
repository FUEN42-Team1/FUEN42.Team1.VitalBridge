using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentTypeCategoryPairRepository
    {
        public void Create(ContentTypeCategoryPair entity);
        public void Delete(ContentTypeCategoryPair entity);
        public IEnumerable<ContentTypeCategoryPair> GetAll();
        public IEnumerable<ContentTypeCategoryPair> GetByCategoryId(int categoryId);
        public IEnumerable<ContentTypeCategoryPair> GetByContentTypeId(int contentTypeId);
        public ContentTypeCategoryPair? GetById(int id);
        public ContentTypeCategoryPair? GetByName(int id);
        public void Update(ContentTypeCategoryPair entity);

    }
}
