using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.EFModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Team1.VitalBridge.Frontend.Controllers
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

        [HttpPost("Get")]
        public async Task<ActionResult<object>> GetComments([FromBody] ArticleCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var query = _context.Comments
                        .Include(c => c.Member)
                        .Where(c => c.ParentCommentId == request.ParentCommentId &&
                        c.ContentId == request.ArticleId &&
                        c.IsEnabled);

                var totalComments = await query.CountAsync();

                var comments = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(c => new ContentArticleCommentDTO
                    {
                        author = c.Member.Name,
                        handle = '@' + c.Member.Name + c.MemberId,
                        time = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                        text = c.Content
                    })
                    .ToListAsync();
               

                return Ok(new
                {
                    comments,
                    totalComments,
                    totalPages = (int)Math.Ceiling((double)totalComments / request.PageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得留言失敗", error = ex.Message });
            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult<object>> CreateComments([FromBody] ArticleCommentRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var comments = await _context.Comments
                    .Where(c => c.ParentCommentId == null && c.ContentId == request.ArticleId && c.IsEnabled == true)
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得留言失敗", error = ex.Message });
            }
        }
    }
}
