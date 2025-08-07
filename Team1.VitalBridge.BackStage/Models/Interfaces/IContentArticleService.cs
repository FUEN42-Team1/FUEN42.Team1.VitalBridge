using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentArticleService
    {
        public Task<IEnumerable<ContentDTO>> GetAllArticlesListAsync();
        public Task<IEnumerable<ContentDTO>> FilterAllArticlesListAsync();


        public Task<ContentArticleDTO> GetArtileByIdAsync(int id);

        public Task AddArticleAsync(ContentArticleCreateDTO article);
        public Task UpdateArticleAsync(ContentArticleEditDTO article);
        public Task DeleteArticleAsync(int id);
    }
}
