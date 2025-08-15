
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class ContentArticleService : IContentArticleService
    {
        private readonly IContentArticleRepository _repository;
        private readonly IMediaRepository _mediaRepository;

        public ContentArticleService(IContentArticleRepository repository, IMediaRepository mediaRepository)
        {
            this._repository = repository;
            this._mediaRepository = mediaRepository;
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
                //CoverPic = article.CoverPic, CoverPic MediaId
                Status = article.Status,
                ViewCount = 0, // Initial view count
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            // todo If CoverPic is provided, set it

            // Save to repository
            await _repository.AddAsync(entity);

            // use regex to find img in Content1 from ckeditor and save it to Media table
            await MediaUpload(entity);
        }

        private async Task MediaUpload(Content entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.Content1))
            {
                var fileIds = System.Text.RegularExpressions.Regex.Matches(entity.Content1, @"<img[^>]+src=""/api/MediasAPI/([^""]+)""")
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToList();
                foreach (var fileId in fileIds)
                {
                    // Create a new Media entity for each fileId
                    var media = new Media
                    {
                        MemberId = 0, // Assuming MemberId is not set for now
                        MediaTypeId = 1,
                        Name = fileId, // Use fileId as the name for simplicity
                        FileId = int.Parse(fileId),
                        Content = entity // Associate with the current content
                    };
                    await _mediaRepository.AddAsync(media);
                }
            }
        }

        public async Task DeleteArticleAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ContentArticleDTO>> GetAllArticlesListAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ContentArticleDTO> GetArticleByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<ContentArticleEditDTO> GetArticleForEditByIdAsync(int id)
        {
            var query = _repository.GetAllWithIncludes();
            var dto = await query
                .Where(c => c.Id == id)
                .Select(c => new ContentArticleEditDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    Content = c.Content1, // Assuming Content1 is the content field
                    ContentCategoryId = c.ContentCategoryId,
                    //CoverPic = null, // Assuming CoverPic is not needed for edit
                    Status = c.Status
                }).FirstOrDefaultAsync();
            if (dto == null)
            {
                throw new KeyNotFoundException($"Article with ID {id} not found.");
            }
            return dto;
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
                .Select(c => new ContentArticleListDTO
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
            if (article == null)
            {
                throw new ArgumentNullException(nameof(article), "Article cannot be null");
            }

            // todo If CoverPic is provided, set it
            // Map DTO to existingEntity
            var entity = await _repository.GetByIdAsync(article.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Article with ID {article.Id} not found.");
            }
            entity.Title = article.Title;
            entity.Content1 = article.Content;
            entity.ContentCategoryId = article.ContentCategoryId;
            entity.Content1 = article.Content;
            entity.Status = article.Status;

            // Save to repository
            await _repository.UpdateAsync(entity);
            // delete all medias with entity.tId
            await MediaDelete(entity);
            // add all new medias with entity content
            await MediaUpload(entity);
        }
        private async Task MediaDelete(Content entity)
        {
            var medias = await _mediaRepository.GetByContentIdAsync(entity.Id);
            if (medias == null)
                return;
            while (medias.Any())
            {
                var media = medias.First();
                await _mediaRepository.DeleteAsync(media.Id);
                medias = await _mediaRepository.GetByContentIdAsync(entity.Id);
            }
        }

    }
}
