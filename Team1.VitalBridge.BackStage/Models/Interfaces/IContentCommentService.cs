using Team1.VitalBridge.BackStage.Models.DTOs;

namespace Team1.VitalBridge.BackStage.Models.Interfaces
{
    public interface IContentCommentService
    {
        public Task<List<ContentCommentDisplayDTO>> GetAllCommentsAsync();

        public Task<List<ContentCommentDisplayDTO>> SearchCommentAsync(ContentCommentListCritriaDTO criteria);
    }
}
