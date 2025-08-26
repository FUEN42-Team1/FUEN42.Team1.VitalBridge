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
        //public async Task<IActionResult> SearchOld([FromQuery] ContentArticleListCritriaDTO? criteria)
        //{
        //    if (criteria == null)
        //    {
        //        criteria = new ContentArticleListCritriaDTO();
        //    }
        //    var results = await _service.SearchArticlesAsync(criteria);

        //    var vm = results.Select(r => new ContentArticleListViewModel
        //    {
        //        Id = r.Id,
        //        Title = r.Title,
        //        CoverPic = r.CoverPic,
        //        CategoryName = r.CategoryName,
        //        MemberName = r.MemberName,
        //        Status = r.Status.ToString(), // Convert int to string for display
        //        ViewCount = r.ViewCount,
        //        CreatedAt = r.CreatedAt,
        //        UpdatedAt = r.UpdatedAt
        //    }).ToList();

        //    return View(vm);
        //}

        public async Task<IActionResult> Search()
        {
            return View();
        }


        // GET: ContentArticles/Create
        public IActionResult Create()
        {
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


            var FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == vm.CoverPicFileName).Id;

            // todo coverpic 還沒存到filetable
            var dto = new ContentArticleCreateDTO
            {
                Title = vm.Title,
                Content = vm.Content,
                ContentCategoryId = vm.ContentCategoryId,
                CoverPic = null,
                CoverPicFileId = FileId,
                MemberId = 1, // Assuming a default member ID for now, replace with actual logic
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

            var dto = await _service.GetArticleForEditByIdAsync(id.Value);
            if (dto == null)
            {
                return NotFound();
            }

            
            var fileName = dto.CoverPicFileId.HasValue
                ? _context.FileStreams.FirstOrDefault(f => f.Id == dto.CoverPicFileId.Value)?.FileName
                : null;

            // Map the DTO to the ViewModel
            var vm = new ContentArticleEditViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                CoverPic = null,
                CoverPicFileName = fileName,
                Content = dto.Content,
                ContentCategoryId = dto.ContentCategoryId,
                //CoverPic = null, // Handle file upload separately
                Status = ((ContentArticleStatus)dto.Status).ToString() // Convert int to string for display
            };

            return View(vm);
        }

        // POST: ContentArticles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContentArticleEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            int? FileId = null;
            if(vm.CoverPicFileName!= null)
                FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == vm.CoverPicFileName).Id;

            // Map the ViewModel to the DTO
            var dto = new ContentArticleEditDTO
            {
                Id = vm.Id,
                Title = vm.Title,
                Content = vm.Content,
                ContentCategoryId = vm.ContentCategoryId,
                CoverPic = null, // Handle file upload separately
                CoverPicFileId = FileId,
                Status = (int)Enum.Parse(typeof(ContentArticleStatus), vm.Status)
            };

            // Call the service to update the article
            await _service.UpdateArticleAsync(dto);

            // Redirect to the Search action after successful update
            return RedirectToAction(nameof(Search));
        }

        // GET: ContentArticles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _repository.DeleteAsync(id.Value);
            // Set the success message in TempData
            TempData["SuccessMessage"] = "Article deleted successfully! 🎉";

            // Redirect to the Index action
            return RedirectToAction("Search", "ContentArticles");
        }

        public async Task<IActionResult> GetCoverPic(int id)
        {
            var content = await _context.Contents.FindAsync(id);
            

            return File("~/images/no-image.png", "image/png"); // fallback if no pic
        }
    }
}
