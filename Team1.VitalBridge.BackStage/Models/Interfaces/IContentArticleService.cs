using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentArticleService
    {
        public Task<IEnumerable<ContentDTO>> GetAllArticlesListAsync();

        public Task<List<ContentArticleListDTO>> SearchArticlesAsync(ContentArticleListCritriaDTO criteria);

        public Task<ContentArticleDTO> GetArticleByIdAsync(int id);

        public Task CreateArticleAsync(ContentArticleCreateDTO article);
        public Task UpdateArticleAsync(ContentArticleEditDTO article);
        public Task DeleteArticleAsync(int id);
    }
}
