using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var vm = dto.Select(c => new ContentCommentListViewModel
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ContentCommentsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
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

        // GET: ContentCommentsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ContentCommentsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
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
    }
}
