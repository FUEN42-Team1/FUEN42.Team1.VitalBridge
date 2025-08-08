using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentArticlesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IContentArticleRepository _repository;
        private readonly IContentArticleService _service;

        public ContentArticlesController(AppDbContext context, IContentArticleRepository repository, IContentArticleService service)
        {
            this._context = context;
            this._repository = repository;
            this._service = service;
        }

        // GET: ContentArticles/Search
        public async Task<IActionResult> Search([FromQuery] ContentArticleListCritriaDTO? criteria)
        {
            if (criteria == null)
            {
                criteria = new ContentArticleListCritriaDTO();
            }
            var results = await _service.SearchArticlesAsync(criteria);

            var vm = results.Select(r => new ContentArticleListViewModel
            {
                Id = r.Id,
                Title = r.Title,
                CoverPic = r.CoverPic,
                CategoryName = r.CategoryName,
                MemberName = r.MemberName,
                Status = r.Status.ToString(), // Convert int to string for display
                ViewCount = r.ViewCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            return View(vm);
        }

        
        // GET: ContentArticles/Create
        public IActionResult Create()
        {
            ViewData["ContentCategoryId"] = new SelectList(_context.ContentCategories, "Id", "Name");
            return View();
        }

        // POST: ContentArticles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentArticleCreateViewModel vm)
        {

            if (!ModelState.IsValid) return View(vm);

            var dto = new ContentArticleCreateDTO
            {
                Title = vm.Title,
                Content = vm.Content,
                ContentCategoryId = vm.ContentCategoryId,
                CoverPic = vm.CoverPic,
                // 修正：將 ContentArticleStatus 轉型為 int
                Status = (int)Enum.Parse(typeof(ContentArticleStatus), vm.Status)
            };

            // Call the service to create the article
            await _service.CreateArticleAsync(dto);
            return RedirectToAction("Search", "ContentArticles");
        }

        // GET: ContentArticles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.Contents.FindAsync(id);
            if (content == null)
            {
                return NotFound();
            }
            ViewData["ContentCategoryId"] = new SelectList(_context.ContentCategories, "Id", "Name", content.ContentCategoryId);
            return View();
        }

        // POST: ContentArticles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MemberId,ContentCategoryId,Title,CoverPic,Content1,Status,ViewCount,UpdatedAt,CreatedAt")] Content content)
        {
            if (id != content.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View();
            return RedirectToAction(nameof(Search));
        }

        // GET: ContentArticles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var content = await _context.Contents
            //    .Include(c => c.ContentCategory)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (content == null)
            //{
            //    return NotFound();
            //}

            return View();
        }

        
    }
}
