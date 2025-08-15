using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentCommentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IContentCommentRepository _repository;
        private readonly IContentCommentService _service;

        public ContentCommentsController(AppDbContext context, IContentCommentRepository repository, IContentCommentService service)
        {
            this._context = context;
            this._repository = repository;
            this._service = service;
        }
        // GET: ContentCommentsController
        public async Task<ActionResult> Index()
        {
            var dto = await _service.GetAllCommentsAsync();
            var vm = dto.Select(c => new ContentCommentListViewModel_I
            {
                Id = c.Id,
                //MemberName = c.MemberName,
                ContentTitle = c.ContentTitle,
                ParentCommentId = c.ParentCommentId,
                Content = c.Content.Length > 10
                 ? c.Content.Substring(0, 7)+ "..."
                 : c.Content.PadRight(10),
                IsPinned = c.IsPinned,
                CreatedAt = c.CreatedAt
            }).ToList();

            return View(vm);
        }

        // GET: ContentCommentsController/Search
        public async Task<ActionResult> Search()
        {
            await _service.GetAllCommentsAsync();
            return View();
            ContentCommentSearchViewModel? SearchVM;
            if (SearchVM == null)
            {
                SearchVM = new ContentCommentSearchViewModel();
            }

            var criteria = new ContentCommentListCritriaDTO
            {
                //ContentTitle = SearchVM.ContentTitle

            };

            var results = await _service.SearchCommentAsync(criteria);

            var vm = results.Select(c => new ContentCommentListViewModel_I
            {
                Id = c.Id,
                //MemberName = c.MemberName,
                ContentTitle = c.ContentTitle,
                ParentCommentId = c.ParentCommentId,
                Content = c.Content.Length > 10
                 ? c.Content.Substring(0, 7) + "..."
                 : c.Content.PadRight(10),
                IsPinned = c.IsPinned,
                CreatedAt = c.CreatedAt
            }).ToList();

            return View(vm);
        }

        

        // GET: ContentCommentsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ContentCommentsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ContentCommentsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ContentCommentsController/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var comment = await _repository.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            var vm = new ContentCommentEditViewModel
            {
                Id = id,
                Content = comment?.Content ?? string.Empty,
                ParentCommentId = comment?.ParentCommentId,
                IsPinned = comment?.IsPinned ?? false,
                CreatedAt = comment?.CreatedAt ?? DateTime.Now,
                //MemberName = comment?.Member?.Name ?? string.Empty, // Assuming Member is a navigation property
                ContentId = comment?.ContentId ?? 0, // Assuming ContentId is the ID of the content the comment belongs to
            };
            return View(vm);
        }

        // POST: ContentCommentsController/Edit/5
        [HttpPost]
        public async Task<ActionResult> Edit(int id, ContentCommentEditViewModel vm)
        {
            try
            {
                var comment = await _repository.GetByIdAsync(id);
                if (comment == null)
                {
                    return NotFound();
                }
                // Update the comment properties
                comment.Content = vm.Content;
                comment.ParentCommentId = vm.ParentCommentId;
                comment.IsPinned = vm.IsPinned;
                comment.CreatedAt = vm.CreatedAt;
                // Assuming MemberName and ContentTitle are not editable, so we don't update them
                comment.ContentId = vm.ContentId; // Assuming this is the ID of the content the comment belongs to
                // Save the changes
                await _repository.UpdateAsync(comment);
                // Redirect to the index or details page after successful edit
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(vm);
            }
        }

        // POST: ContentCommentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var comments = await _repository.GetByIdAsync(id);
                if (comments == null)
                {
                    return NotFound();
                }
                await _repository.DeleteAsync(id);
                // Set the success message in TempData
                TempData["SuccessMessage"] = "Comment deleted successfully! 🎉";
                // Redirect to the index or another appropriate page after successful deletion
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
