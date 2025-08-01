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

        

    }
}
