using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var contentTypeCategoryPairDisplayViewModel = dto.Select(pair => new ContentTypeCategoryPairDisplayViewModel
            {
                Id = pair.Id,
                ContentTypeName = pair.ContentTypeName,
                ContentTypeDisplayOrder = pair.ContentTypeDisplayOrder,

                ParentContentCategoryName = string.IsNullOrEmpty(pair.ParentContentCategoryName)
                ? "NoParent"
                : pair.ParentContentCategoryName,
                ParentContentCategoryDisplayOrder = pair.ParentContentCategoryDisplayOrder == null
                ? -1
                : pair.ParentContentCategoryDisplayOrder,

                ContentCategoryName = pair.ContentCategoryName,
                ContentCategoryDisplayOrder = pair.ContentCategoryDisplayOrder,

                IsEnabledStatus = pair.IsEnabled ? "Enabled" : "Disabled"
            }).ToList();

            return View(contentTypeCategoryPairDisplayViewModel);
        }
    }
}
