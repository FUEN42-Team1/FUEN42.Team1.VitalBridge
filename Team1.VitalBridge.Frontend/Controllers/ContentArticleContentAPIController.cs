using Azure.Core;
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
                    .OrderByDescending(a => a.CreatedAt)
                    .Include(a => a.ContentCategory) // Eagerly load the category
                    .Include(a => a.Member)         // Eagerly load the member
                    .Include(a => a.CoverPicFile) // Eagerly load the cover picture file
                    .Where(a => (request.CategoryId == null || a.ContentCategoryId == request.CategoryId) && a.Status == 1) // Filter articles based on the request parameters
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var articledtos = articles.Select(a => new ContentArticleDisplayDTO
                {
                    Id = a.Id,
                    Title = a.Title,
                    CoverPic = null,
                    CoverPicFileName = a.CoverPicFile == null ? null : a.CoverPicFile.FileName,
                    CategoryId = a.ContentCategory.Id,
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

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetArticleById(int id)
        {
            try
            {

                // Then get the paginated articles
                var article = await _context.Contents
                    .Include(a => a.ContentCategory) // Eagerly load the category
                    .Include(a => a.Member)         // Eagerly load the member
                    .Include(a => a.CoverPicFile) // Eagerly load the cover picture file
                    .FirstOrDefaultAsync(a => a.Id == id);

                var articledto = new ContentArticleContentDisplayDTO
                {
                    Id = article.Id,
                    Title = article.Title,
                    CoverPicFileName = article.CoverPicFile == null ? null : article.CoverPicFile.FileName,
                    CategoryId = article.ContentCategory.Id,
                    Category = article.ContentCategory.Name,
                    Content = article.Content1,
                    Author = article.Member.Name,
                    Date = article.CreatedAt
                };

                // Return the response in the format expected by your Vue.js component
                var response = articledto;
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while fetching articles." });
            }
            
        }

        [HttpPost("ViewPlusOne")]
        public async Task<ActionResult<object>> GetArticleViewPlusOne([FromBody] ContentArticleViewPlusOneDTO request)
        {
            try
            {

                // Then get the paginated articles
                var article = await _context.Contents
                    .FirstOrDefaultAsync(a => a.Id == request.articleId);

                article.ViewCount += 1;
                await _context.SaveChangesAsync();
                
                return Ok(article);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while making articles ViewCount plus one." });
            }
        }

        [HttpGet("Trending")]
        public async Task<ActionResult<object>> GetArticleTrending()
        {
            try
            {
                // Then get the paginated articles
                var articles = await _context.Contents
                    .Where(a=>a.Status==1)
                    .OrderByDescending(a => a.ViewCount)
                    .Take(4)
                    .Select(a => new
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Url = "ArticleContent.html?id="+ a.Id
                    })
                    .ToListAsync();


                return Ok(articles);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred while fetching trending articles." });
            }
        }
    }
}
