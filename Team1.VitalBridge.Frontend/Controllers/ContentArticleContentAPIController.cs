using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
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

        [HttpPost]
        public async Task<ActionResult<object>> GetArticles([FromBody] ArticleRequestDTO request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // First, get the total count of articles that match the filter
                var totalCount = await _context.Contents
                    .Where(a => (request.CategoryId == null || a.ContentCategoryId == request.CategoryId) && a.Status == 1)
                    .CountAsync();

                // Then get the paginated articles
                var articles = await _context.Contents
                    .Include(a => a.ContentCategory) // Eagerly load the category
                    .Include(a => a.Member)         // Eagerly load the member
                    .Where(a => (request.CategoryId == null || a.ContentCategoryId == request.CategoryId) && a.Status == 1) // Filter articles based on the request parameters
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var articledtos = articles.Select(a => new ContentArticleDisplayDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    CoverPic = a.CoverPic != null ? Convert.ToBase64String(a.CoverPic) : null, // Convert byte array to Base64 string
                    Category = a.ContentCategory.Name,
                    Author = a.Member.Name,
                    Date = a.CreatedAt
                }).ToList();

                // Return the response in the format expected by your Vue.js component
                var response = new
                {
                    articles = articledtos,
                    totalCount = totalCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while fetching articles." });
            } 
        }
    }
}
