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
                        id = c.Id,
                        author = c.Member.Name,
                        handle = '@' + c.Member.Name + c.MemberId,
                        time = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                        text = c.Content,
                        repliesCount = _context.Comments.Count(r => r.ParentCommentId == c.Id && r.IsEnabled)
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

        [HttpPost("GetReplies")]
        public async Task<ActionResult<object>> GetCommentReplies([FromBody] ArticleCommentReplyDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var query = _context.Comments
                        .Include(c => c.Member)
                        .Where(c => c.ParentCommentId == request.CommentId &&
                        c.IsEnabled);


                var replies = await query
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new ContentArticleCommentDTO
                    {
                        id = c.Id,
                        author = c.Member.Name,
                        handle = '@' + c.Member.Name + c.MemberId,
                        time = c.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                        text = c.Content,
                        repliesCount = _context.Comments.Count(r => r.ParentCommentId == c.Id && r.IsEnabled)
                    })
                    .ToListAsync();


                return Ok(replies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得留言失敗", error = ex.Message });
            }
        }


        [HttpPost("Create")]
        public async Task<ActionResult<object>> CreateComments([FromBody] ArticleCommentCreateDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var entity = new Team1.VitalBridge.Frontend.Models.EFModels.Comment
                {
                    MemberId = request.userId,
                    ContentId = request.articleId,
                    ParentCommentId = request.parentCommentId,
                    Content = request.text,
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Comments.AddAsync(entity);
                await _context.SaveChangesAsync(new CancellationToken());

                var createdComment = new ContentArticleCommentDTO
                {
                    id = entity.Id,
                    author = _context.Users.FirstOrDefault(u => u.Id == entity.MemberId)?.Name,
                    handle = '@' + _context.Users.FirstOrDefault(u => u.Id == entity.MemberId)?.Name + entity.MemberId,
                    time = entity.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    text = entity.Content,
                    repliesCount = 0
                };

                return Ok(createdComment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "新增留言失敗", error = ex.Message, });
            }
        }
    }
}
