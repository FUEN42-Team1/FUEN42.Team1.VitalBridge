using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.EFModels; // 請確保這裡的 using 正確
using Team1.VitalBridge.BackStage.Models.ViewModels;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class FeatureServicesController : Controller
    {
        private readonly AppDbContext _context;

        public FeatureServicesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // 修正：使用 Include 載入關聯的 FileStream
            var services = _context.FeatureServices
                                   .Include(s => s.File) // 載入 File 導覽屬性
                                   .Select(s => new FeatureServiceViewModel
                                   {
                                       Id = s.Id,
                                       Name = s.Name,
                                       // 修正：從 File 實體取得檔名，若 FileId 為 null 則 ImageUrl 也為 null
                                       ImageUrl = s.File.FileName,
                                       IsActive = s.IsActive
                                   })
                                   .ToList();
            return View(services);
        }

        [HttpPost]
        public IActionResult Create([FromBody] FeatureServiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 修正：從 ImageUrl 找到對應的 FileId
                var file = _context.FileStreams.FirstOrDefault(f => f.FileName == model.ImageUrl);
                if (file == null)
                {
                    return Json(new { success = false, message = "圖片檔案不存在。" });
                }

                var featureService = new FeatureService
                {
                    Name = model.Name,
                    FileId = file.Id, // 將 FileId 存入實體模型
                    IsActive = true
                };
                _context.FeatureServices.Add(featureService);
                _context.SaveChanges();
                return Json(new { success = true, message = "新增成功！" });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        [HttpPost]
        public IActionResult Edit([FromBody] FeatureServiceViewModel model)
        {
            if (ModelState.IsValid)
            {
                var featureService = _context.FeatureServices.Find(model.Id);
                if (featureService != null)
                {
                    // 修正：從 ImageUrl 找到對應的 FileId
                    var file = _context.FileStreams.FirstOrDefault(f => f.FileName == model.ImageUrl);
                    if (file == null)
                    {
                        return Json(new { success = false, message = "圖片檔案不存在。" });
                    }

                    featureService.Name = model.Name;
                    featureService.FileId = file.Id; // 更新 FileId
                    _context.FeatureServices.Update(featureService);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "編輯成功！" });
                }
                return Json(new { success = false, message = "找不到該服務項目。" });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id, bool isActive)
        {
            var service = _context.FeatureServices.Find(id);
            if (service == null)
            {
                return Json(new { success = false, message = "找不到該服務項目。" });
            }

            service.IsActive = isActive;
            _context.SaveChanges();
            return Json(new { success = true, message = "狀態已成功變更。" });
        }
    }
}