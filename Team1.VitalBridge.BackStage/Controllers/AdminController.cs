using System.Drawing;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Service;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class AdminController : Controller
    {
        private readonly PlateService _service;
        private readonly PlateImageService _imageService;
        private readonly AppDbContext _context;
        private readonly NotifyService _notifyService;
        private readonly CategoryService _categoryService;

        public AdminController(PlateService service,PlateImageService imageService,AppDbContext context,NotifyService notifyService,CategoryService categoryService)
        {
            this._service = service;
            this._imageService = imageService;
            this._context = context;
            this._notifyService = notifyService;
            this._categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminPlate()
        {
            
            var plates = _service.GetPlates()
                .Select(p => new AdminPlateViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Enable = p.Enable,
                    Route = p.Route,
                })
                .ToList();
            return View(plates);
        }
        public IActionResult EditPlate(int id)
        {
            var plate = _service.GetPlate(id);

            return View(plate);
        }
        public IActionResult CreatePlate()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePlate(AddBoxViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _service.CreatePlate(vm);
            return RedirectToAction("AdminPlate");
        }

        public IActionResult EditPlateImage(int id)
        {
            PlateImage plateImage = _imageService.GetPlateImageById(id);

            return View(plateImage);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePlateImage(CreatePlateImageViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            _imageService.CreatePlateImage(vm);
            return RedirectToAction("EditPlate", new {id= vm.PlateId });
        }

        public IActionResult AdminNotifys()
        {
            var Notifys = _notifyService.GetAllNotify();
            return View(Notifys);
        }
        public IActionResult AdminNotifyCategory()
        {
            var Categories = _categoryService.getAllCategories();
            return View(Categories);
        }
    }
}
