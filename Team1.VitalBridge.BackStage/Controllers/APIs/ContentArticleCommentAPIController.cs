using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentArticleCommentAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContentArticleCommentAPIController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpGet]
        public async Task<IEnumerable<ContentArticleCommentDisplayDTO?>> Get([FromQuery] ContentArticleCommentCriteriaDTO criteria)
        {
            var query = _context.Comments.AsQueryable();

            if (!string.IsNullOrEmpty(criteria.ArticleTitle))
            {
                query = query.Where(c => c.ContentNavigation.Title.Contains(criteria.ArticleTitle));
            }

            if (!string.IsNullOrEmpty(criteria.MemberName))
            {
                query = query.Where(c => c.Member.Name.Contains(criteria.MemberName));
            }

            if (!string.IsNullOrEmpty(criteria.CommentContent))
            {
                query = query.Where(c => c.Content.Contains(criteria.CommentContent));
            }

            if (criteria.Status == "enabled")
            {
                query = query.Where(c => c.IsEnabled);
            }
            else if (criteria.Status == "disabled")
            {
                query = query.Where(c => !c.IsEnabled);
            }

            //map the query results to DTOs
            var result = query.Select(c => new ContentArticleCommentDisplayDTO
            {
                Id = c.Id,
                ArticleTitle = c.ContentNavigation.Title,
                ArticleId = c.ContentNavigation.Id,
                MemberName = c.Member.Name,
                MemberId = c.Member.Id,
                ParentCommentId = c.ParentCommentId,
                CommentContent = c.Content,
                IsEnabled = c.IsEnabled,
                CreatedAt = c.CreatedAt
            }).ToList();

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEnableStatus([FromBody] ContentArticleCommentEditDTO request)
        {
            var comment = await _context.Comments.FindAsync(request.Id);
            if (comment == null)
                return NotFound();


            comment.IsEnabled = !comment.IsEnabled; // Toggle the status
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();

            return Ok(comment); // or just Ok()
        }
        // Additional methods for POST, PUT, DELETE can be added here as needed.
    }
}
