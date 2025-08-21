using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentArticleCategoryAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContentArticleCategoryAPIController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpGet("{id?}")]
        public async Task<ActionResult<IEnumerable<ContentCategoryDTO>>> GetCategories(int? id)
        {
            var categories = await _context.ContentCategories
                                   .Where(c => c.ParentCategoryId == id)
                                   .ToListAsync();

            if (!categories.Any())
            {
                return NotFound();
            }

            var categoryDtos = new List<ContentCategoryDTO>();
            foreach (var category in categories)
            {
                var hasSubcategories = await _context.ContentCategories
                                                     .AnyAsync(sub => sub.ParentCategoryId == category.Id);

                categoryDtos.Add(new ContentCategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Link = category.Id.ToString(), // Note: changed from 'link' to 'Link' for PascalCase convention
                    HasSubcategories = hasSubcategories
                });
            }

            return categoryDtos; // Returns a 200 OK with the list of categories.
        }
    }
}
