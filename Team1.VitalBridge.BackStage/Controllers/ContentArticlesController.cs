using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentArticlesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IContentArticleRepository _repository;


        public ContentArticlesController(AppDbContext context, IContentArticleRepository repository)
        {
            this._context = context;
            this._repository = repository;
        }

        // GET: ContentArticles
        public async Task<IActionResult> Index()
        {
            var entities = await _repository.GetAllAsync();

            // Entities to ViewModel conversion can be done here if needed
            var vm = entities.Select(e => new ContentArticleListViewModel
            {
                Id = e.Id,
                Title = e.Title,
                CoverPic = e.CoverPic,
                ContentCategoryId = e.ContentCategoryId,
                ContentCategoryName = e.ContentCategory?.Name, // Assuming ContentCategory is a navigation property
                ViewCount = e.ViewCount,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            }).ToList();
            return View(vm);
        }

        // GET: ContentArticles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var content = await _context.Contents
                .Include(c => c.ContentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
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
        public async Task<IActionResult> Create([Bind("Id,MemberId,ContentCategoryId,Title,CoverPic,Content1,Status,ViewCount,UpdatedAt,CreatedAt")] Content content)
        {
            if (ModelState.IsValid)
            {
                _context.Add(content);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ContentCategoryId"] = new SelectList(_context.ContentCategories, "Id", "Name", content.ContentCategoryId);
            return View(content);
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
            return RedirectToAction(nameof(Index));
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
