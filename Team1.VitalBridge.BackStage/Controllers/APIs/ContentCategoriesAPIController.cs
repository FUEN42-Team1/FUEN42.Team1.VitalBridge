using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentCategoriesAPIController : ControllerBase
    {
        private readonly IContentCategoryRepository _repository;

        public ContentCategoriesAPIController(IContentCategoryRepository repository)
        {
            this._repository = repository;
        }

        // GET: api/ContentCategoriesAPI
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContentCategoryDTO>>> GetContentCategories()
        {
            var entities = await _repository.GetAllAsync();

            if (entities == null || !entities.Any())
            {
                return NotFound();
            }
            // Convert EF models to DTOs
            var dtoList = entities.Select(c => new ContentCategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                IsEnabled = c.IsEnabled,
                DisplayOrder = c.DisplayOrder,
                UpdatedAt = c.UpdatedAt,
                CreatedAt = c.CreatedAt
            }).ToList();

            return dtoList;
        }

        // GET: api/ContentCategoriesAPI/SubCategories/5
        [HttpGet("SubCategories/{ParentId}")]
        public async Task<IEnumerable<ContentCategoryDTO?>> GetContentSubCategories(int? ParentId)
        {
            var entities = await _repository.GetByParentIdAsync(ParentId);

            if (entities == null || !entities.Any() || entities.Count()==0)
            {
                return null;
            }

            // Convert EF models to DTOs
            var dtoList = entities.Select(c => new ContentCategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                IsEnabled = c.IsEnabled,
                DisplayOrder = c.DisplayOrder,
                UpdatedAt = c.UpdatedAt,
                CreatedAt = c.CreatedAt
            }).ToList();

            return dtoList;
        }

        // GET: api/ContentCategoriesAPI/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ContentCategoryDTO>> GetContentCategory(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            // Convert EF model to DTO
            var dto = new ContentCategoryDTO
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentCategoryId = entity.ParentCategoryId,
                IsEnabled = entity.IsEnabled,
                DisplayOrder = entity.DisplayOrder,
                UpdatedAt = entity.UpdatedAt,
                CreatedAt = entity.CreatedAt
            };

            return dto;
        }

        // PUT: api/ContentCategoriesAPI/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContentCategory(int id, ContentCategoryDTO contentCategoryDTO)
        {
            

            return NoContent();
        }

        // POST: api/ContentCategoriesAPI
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ContentCategory>> PostContentCategory(ContentCategory contentCategory)
        {
            return NoContent();
        }

        // DELETE: api/ContentCategoriesAPI/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContentCategory(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }

       
    }
}
