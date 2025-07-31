using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interfaces;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class ContentsController : Controller
    {
        private readonly IContentTypeCategoryPairService _service;
        public ContentsController(IContentTypeCategoryPairService service)
        {
            this._service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ContentsIndex()
        {
            return View();
        }

        public IActionResult ContentTypeCategoryPairsIndex()
        {
            var dto = _service.GetAll();
            var contentTypeCategoryPairDisplayViewModel=dto.Select(pair=>new ContentTypeCategoryPairDisplayViewModel
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

        public IActionResult CreateContentTypeCategoryPair()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateContentTypeCategoryPair(ContentTypeCategoryPairCreateViewModel vm)
        {
            if (ModelState.IsValid == false) return View(vm);

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

            return View();
        }

    }
}
