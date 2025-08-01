using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Repositories
{
    public class ContentTypeCategoryPairRepository : IContentTypeCategoryPairRepository
    {
        private readonly AppDbContext _context;

        public ContentTypeCategoryPairRepository(AppDbContext context)
        {
            this._context = context;
        }
        public void Create(ContentTypeCategoryPair entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(ContentTypeCategoryPair entity)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentTypeCategoryPair> GetAll()
        {
            return _context.ContentTypeCategoryPairs
                .AsNoTracking()
                .Include(pair => pair.ContentType)
                .Include(pair => pair.ContentCategory)
                    .ThenInclude(paircc => paircc.ParentCategory)
                .ToList();
        }

        public IEnumerable<ContentTypeCategoryPair> GetByCategoryId(int categoryId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ContentTypeCategoryPair> GetByContentTypeId(int contentTypeId)
        {
            throw new NotImplementedException();
        }

        public ContentTypeCategoryPair? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public ContentTypeCategoryPair? GetByName(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(ContentTypeCategoryPair entity)
        {
            throw new NotImplementedException();
        }
    }
}
