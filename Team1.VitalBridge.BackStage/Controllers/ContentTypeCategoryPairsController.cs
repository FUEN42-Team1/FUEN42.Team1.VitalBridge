using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentTypeCategoryPairsController : Controller
    {
        private readonly IContentTypeCategoryPairService _service;

        public ContentTypeCategoryPairsController(IContentTypeCategoryPairService service)
        {
            this._service = service;
        }

        // GET: ContentTypeCategoryPairsController
        public ActionResult Index()
        {
            var dto = _service.GetAll();
            var contentTypeCategoryPairDisplayViewModel = dto.Select(pair => new ContentTypeCategoryPairDisplayViewModel
            {
                Id = pair.Id,
                ContentTypeName = pair.ContentTypeName,
                ContentTypeDisplayOrder = pair.ContentTypeDisplayOrder,

                ParentContentCategoryName = pair.ParentContentCategoryName ?? "No Parent Category",
                ParentContentCategoryDisplayOrder = pair.ParentContentCategoryDisplayOrder ?? -1,

                ContentCategoryName = pair.ContentCategoryName,
                ContentCategoryDisplayOrder = pair.ContentCategoryDisplayOrder,

                IsEnabledStatus = pair.IsEnabled ? "Enabled" : "Disabled"
            }).ToList();

            return View(contentTypeCategoryPairDisplayViewModel);
        }

        // GET: ContentTypeCategoryPairsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ContentTypeCategoryPairsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ContentTypeCategoryPairsController/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(ContentTypeCategoryPairCreateViewModel vm)
        {
            if (ModelState.IsValid == false) return View(vm);

            try
            {
                // Convert ViewModel to DTO
                var dto = new ContentTypeCategoryPairCreateDTO
                {
                    ContentTypeId = vm.ContentTypeId,
                    ContentCategoryId = vm.ContentCategoryId,
                    NewCategoryName = vm.NewCategoryName,
                    NewCategoryParentCategoryId = vm.NewCategoryParentCategoryId,
                    IsNewCategoryEnabled = vm.IsNewCategoryEnabled,
                    NewCategoryDisplayOrder = vm.NewCategoryDisplayOrder,
                    IsEnabled = vm.IsEnabled
                };

                // Call the service to create the content type category pair
                _service.Create(dto);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ContentTypeCategoryPairsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ContentTypeCategoryPairsController/Edit/5
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

        // GET: ContentTypeCategoryPairsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ContentTypeCategoryPairsController/Delete/5
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
