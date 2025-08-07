using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class ContentArticleService : IContentArticleService
    {
        private readonly IContentArticleRepository _repository;

        public ContentArticleService(IContentArticleRepository repository)
        {
            this._repository = repository;
        }
        public Task AddArticleAsync(ContentArticleCreateDTO article)
        {
            throw new NotImplementedException();
        }

        public Task DeleteArticleAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContentDTO>> FilterAllArticlesListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ContentDTO>> GetAllArticlesListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ContentArticleDTO> GetArtileByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateArticleAsync(ContentArticleEditDTO article)
        {
            throw new NotImplementedException();
        }
    }
}
