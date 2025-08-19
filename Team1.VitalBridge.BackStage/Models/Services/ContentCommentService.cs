using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class ContentCommentService : IContentCommentService
    {
        private readonly IContentCommentRepository _repository;

        public ContentCommentService(IContentCommentRepository repository)
        {
            this._repository = repository;
        }
        public async Task<List<ContentCommentDisplayDTO>> GetAllCommentsAsync()
        {
            var entities = await _repository.GetAllIncludeAsync();
            var dto = entities.Select(c => new ContentCommentDisplayDTO
            {
                Id = c.Id,
                //MemberName = c.Member.Name,
                ContentTitle = c.ContentNavigation.Title,
                ParentCommentId = c.ParentCommentId,
                Content = c.Content,
                IsEnabled = c.IsEnabled,
                CreatedAt = c.CreatedAt
            }).ToList();
            return dto;
        }

        public async Task<List<ContentCommentDisplayDTO>> SearchCommentAsync(ContentCommentListCritriaDTO criteria)
        {
            throw new NotImplementedException();
        }
    }
}
