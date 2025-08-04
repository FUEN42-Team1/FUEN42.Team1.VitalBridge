using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentCategoriesController : Controller
    {
        private readonly IContentCategoryService service;

        public ContentCategoriesController(IContentCategoryService _service)
        {
            this.service = _service;
        }

        // GET: ContentCategories
        public async Task<IActionResult> Index()
        {
            var dtoList = await service.GetSubCategoriesAsync(null);
            var vm = dtoList.Select(c => new ContentCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                IsEnabled = c.IsEnabled
            }).ToList();

            return View(vm);
        }

        // GET: ContentCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var contentCategory = await _context.ContentCategories
            //    .Include(c => c.ParentCategory)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (contentCategory == null)
            //{
            //    return NotFound();
            //}

            return View();
        }

        // GET: ContentCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ContentCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentCategoryId,IsEnabled,DisplayOrder,UpdatedAt,CreatedAt")] ContentCategory contentCategory)
        {

            return View();
        }

        // GET: ContentCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var contentCategory = await _context.ContentCategories.FindAsync(id);
            //if (contentCategory == null)
            //{
            //    return NotFound();
            //}
            return View();
        }

        // POST: ContentCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentCategoryId,IsEnabled,DisplayOrder,UpdatedAt,CreatedAt")] ContentCategory contentCategory)
        {
            if (id != contentCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(contentCategory);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
                return RedirectToAction(nameof(Index));
            }

            return View();
        }

        // GET: ContentCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var contentCategory = await _context.ContentCategories
            //    .Include(c => c.ParentCategory)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (contentCategory == null)
            //{
            //    return NotFound();
            //}


            return View();
        }

        // POST: ContentCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            return View();
        }

    }
}
