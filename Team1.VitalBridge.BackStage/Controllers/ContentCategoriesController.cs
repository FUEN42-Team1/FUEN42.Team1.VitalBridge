using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentCategoriesController : Controller
    {
        private readonly IContentCategoryService _service;

        public ContentCategoriesController(IContentCategoryService service)
        {
            this._service = service;
        }

        // GET: ContentCategories
        public async Task<IActionResult> Index()
        {
            var dtoList = await _service.GetSubCategoriesAsync(null);
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

        // GET: ContentCategories/Create?parentId=
        public async Task<IActionResult> Create()
        {
            var vm = new ContentCategoryCreateViewModel();

            string parentId = Request.Query["parentId"];

            if (int.TryParse(parentId, out int parentCategoryId))
            {
                vm.ParentCategoryId = parentCategoryId;
            }
            else
            {
                vm.ParentCategoryId = null;
            }

            if (vm.ParentCategoryId != null)
            {
                // 如果有 parentId，則查詢父類別的名稱
                var parentCategory = await _service.GetCategoryByIdAsync(vm.ParentCategoryId.Value);
                if (parentCategory != null)
                {
                    vm.ParentCategoryName = parentCategory.Name;
                }
            }
            else
            {
                vm.ParentCategoryName = "沒有父類別"; // 沒有父類別時
            }

            vm.IsEnabled = true; // 預設為啟用狀態

            return View(vm); //  Now the model will not be null
        }

        // POST: ContentCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContentCategoryCreateViewModel vm)
        {
            // 若驗證失敗,就再度顯示表單
            if (ModelState.IsValid == false) return View(vm);

            // 將 ViewModel 轉換為 DTO
            var dto = new ContentCategoryCreateDTO
            {
                Name = vm.Name,
                ParentCategoryId = vm.ParentCategoryId,
                IsEnabled = vm.IsEnabled,
                DisplayOrder = vm.DisplayOrder
            };

            await _service.AddCategoryAsync(dto);

            return RedirectToAction("Index");
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
