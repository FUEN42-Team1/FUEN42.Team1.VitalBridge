using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentArticleContentAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContentArticleContentAPIController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<ContentArticleContentDisplayDTO?>> Get([FromQuery] ContentArticleContentCriteriaDTO criteria)
        {
            var query = _context.Contents.Include(n => n.CoverPicFile).AsQueryable().Select(n=>new Content
            {
                Id=n.Id,
                MemberId = n.MemberId,
                ContentCategoryId = n.ContentCategoryId,
                Title = n.Title,
                CoverPic = n.CoverPic,
                Content1=n.Content1,
                Status = n.Status,
                ViewCount = n.ViewCount,
                UpdatedAt = n.UpdatedAt,
                CreatedAt = n.CreatedAt,
                CoverPicFileId = n.CoverPicFileId,
                Comments = n.Comments,
                ContentCategory = n.ContentCategory,
                CoverPicFile = new Team1.VitalBridge.BackStage.Models.EFModels.FileStream
                {
                    Id = n.CoverPicFile.Id,
                    FileName = n.CoverPicFile.FileName,
                },
                Media = n.Media,
                Member=n.Member
            });

            if (!string.IsNullOrEmpty(criteria.Keyword))
            {
                query = query.Where(c => c.Title.Contains(criteria.Keyword) ||
                                         c.ContentCategory.Name.Contains(criteria.Keyword) ||
                                         c.Member.Name.Contains(criteria.Keyword));
            }

            if (criteria.Status.HasValue)
            {
                query = query.Where(c => c.Status == criteria.Status.Value);
            }

            if (criteria.MinViewCount.HasValue)
            {
                query = query.Where(c => c.ViewCount >= criteria.MinViewCount.Value);
            }
            if (criteria.MaxViewCount.HasValue)
            {
                query = query.Where(c => c.ViewCount <= criteria.MaxViewCount.Value);
            }

            if (criteria.CreatedAtStart.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= criteria.CreatedAtStart.Value);
            }
            if (criteria.CreatedAtEnd.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= criteria.CreatedAtEnd.Value);
            }

            if (criteria.UpdatedAtStart.HasValue)
            {
                query = query.Where(c => c.UpdatedAt >= criteria.UpdatedAtStart.Value);
            }
            if (criteria.UpdatedAtEnd.HasValue)
            {
                query = query.Where(c => c.UpdatedAt <= criteria.UpdatedAtEnd.Value);
            }


            //map the query results to DTOs
            var result = query
                .Select(c=> new ContentArticleContentDisplayDTO
            {
                Id = c.Id,
                Title = c.Title,
                CoverUrl = null,
                CoverPicFileName = c.CoverPicFile.FileName,
                Category = c.ContentCategory.Name,
                MemberName = c.Member.Name,
                Status = c.Status.ToString(),
                Views = c.ViewCount,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }
            )
            .ToList();

            return result;
        }
    }
}
