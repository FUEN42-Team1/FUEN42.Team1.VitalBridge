using Humanizer;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
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

        public async Task CreateArticleAsync(ContentArticleCreateDTO article)
        {
            if (article == null)
            {
                throw new ArgumentNullException(nameof(article), "Article cannot be null");
            }

            // Map DTO to Entity
            var entity = new Content
            {
                Title = article.Title,
                Content1 = article.Content,
                ContentCategoryId = article.ContentCategoryId,
                CoverPic = article.CoverPic, // Assuming CoverPic is a byte array
                Status = article.Status,
                ViewCount = 0, // Initial view count
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            // Save to repository
            await _repository.AddAsync(entity);
        }

        public async Task DeleteArticleAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<ContentDTO>> GetAllArticlesListAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ContentArticleDTO> GetArticleByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ContentArticleListDTO>> SearchArticlesAsync(ContentArticleListCritriaDTO criteria)
        {
            var query = _repository.GetAllWithIncludes();

            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                // Ensure keyword is trimmed to avoid leading/trailing spaces
                var keyword = criteria.Keyword.Trim();

                // Keyword search (Title, CategoryName, MemberName)
                query = query.Where(c =>
                    c.Title.Contains(keyword) ||
                    c.ContentCategory.Name.Contains(keyword));
                //|| c.Member.Name.Contains(keyword)); should be included but not yet linked
            }

            // Filter by Status
            if (criteria.Status.HasValue)
            {
                query = query.Where(c => c.Status == criteria.Status.Value);
            }

            // Filter by ViewCount range
            if (criteria.MinViewCount.HasValue)
            {
                query = query.Where(c => c.ViewCount >= criteria.MinViewCount.Value);
            }
            if (criteria.MaxViewCount.HasValue)
            {
                query = query.Where(c => c.ViewCount <= criteria.MaxViewCount.Value);
            }

            // Filter by CreatedAt range
            if (criteria.CreatedAtStart.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= criteria.CreatedAtStart.Value);
            }
            if (criteria.CreatedAtEnd.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= criteria.CreatedAtEnd.Value);
            }

            // Filter by UpdatedAt range
            if (criteria.UpdatedAtStart.HasValue)
            {
                query = query.Where(c => c.UpdatedAt >= criteria.UpdatedAtStart.Value);
            }
            if (criteria.UpdatedAtEnd.HasValue)
            {
                query = query.Where(c => c.UpdatedAt <= criteria.UpdatedAtEnd.Value);
            }

            var dto = await query.OrderBy(c => c.ContentCategory.DisplayOrder)
                .Select(c=> new ContentArticleListDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    CoverPic = c.CoverPic, // CoverPic datatype byte[]
                    ContentCategoryId = c.ContentCategoryId,
                    CategoryName = c.ContentCategory.Name,
                    //MemberId = c.Member.Id, // Assuming MemberId is a navigation property
                    //MemberName = c.Member.Name, // Assuming Member is a navigation property
                    Status = c.Status,
                    ViewCount = c.ViewCount,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                }).ToListAsync();
            return dto;
        }

        public async Task UpdateArticleAsync(ContentArticleEditDTO article)
        {
            throw new NotImplementedException();
        }
    }
}
