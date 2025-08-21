using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var query = _context.Contents.AsQueryable();

            if(!string.IsNullOrEmpty(criteria.Keyword))
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
            var result = query.Select(c => new ContentArticleContentDisplayDTO
            {
                Id = c.Id,
                Title = c.Title,
                CoverUrl = c.CoverPic != null ? Convert.ToBase64String(c.CoverPic) : null,
                Category = c.ContentCategory.Name,
                MemberName = c.Member.Name,
                Status = c.Status.ToString(),
                Views = c.ViewCount,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            return result;
        }
    }
}
