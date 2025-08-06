using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels; // 確保這裡引用了所有 ViewModel
using System.Linq;
using System.Threading.Tasks;

namespace Team1.VitalBridge.BackStage.Controllers
{
    public class FeatureServicesController : Controller
    {
        private readonly AppDbContext _context;

        public FeatureServicesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: FeatureServices
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchQuery = "")
        {
            // 處理查詢
            var query = _context.FeatureServices.AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(fs => fs.Name.Contains(searchQuery));
            }

            // 處理排序
            query = query.OrderBy(fs => fs.Id);

            // 計算分頁
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 取得分頁資料
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(fs => new FeatureServiceViewModel
                {
                    Id = fs.Id,
                    Name = fs.Name,
                    // 修正：從 File 實體取得檔名，若 FileId 為 null 則 ImageUrl 也為 null
                    ImageUrl = fs.FileId.HasValue ? _context.FileStreams.FirstOrDefault(f => f.Id == fs.FileId.Value).FileName : null,
                    IsActive = fs.IsActive
                })
                .ToListAsync();

            // 將資料和分頁資訊傳給 View
            var result = new PaginatedResult<FeatureServiceViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(result);
        }

        // POST: FeatureServices/Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeatureServiceViewModel model) // 修正：使用 CreateFeatureServiceViewModel
        {
            if (ModelState.IsValid)
            {
                // 檢查名稱是否重複
                var isNameExists = await _context.FeatureServices.AnyAsync(fs => fs.Name == model.Name);
                if (isNameExists)
                {
                    return Json(new { success = false, message = "特色服務名稱已存在，請使用其他名稱。" });
                }

                // 從 ImageUrl 找到對應的 FileId
                var file = await _context.FileStreams.FirstOrDefaultAsync(f => f.FileName == model.ImageUrl);
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
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "新增成功！" });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        // POST: FeatureServices/Edit
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] FeatureServiceViewModel model) // 修正：使用 FeatureServiceViewModel
        {
            if (ModelState.IsValid)
            {
                var featureService = await _context.FeatureServices.FindAsync(model.Id);
                if (featureService == null)
                {
                    return Json(new { success = false, message = "找不到該服務項目。" });
                }

                // 檢查名稱是否重複，但要排除自己本身
                var isNameExists = await _context.FeatureServices.AnyAsync(fs => fs.Name == model.Name && fs.Id != model.Id);
                if (isNameExists)
                {
                    return Json(new { success = false, message = "特色服務名稱已存在，請使用其他名稱。" });
                }

                // 從 ImageUrl 找到對應的 FileId
                var newFile = await _context.FileStreams.FirstOrDefaultAsync(f => f.FileName == model.ImageUrl);
                if (newFile == null)
                {
                    return Json(new { success = false, message = "圖片檔案不存在。" });
                }

                featureService.Name = model.Name;
                featureService.FileId = newFile.Id; // 更新 FileId

                _context.FeatureServices.Update(featureService);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "編輯成功！" });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        // POST: FeatureServices/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, [FromForm] bool isActive)
        {
            var featureService = await _context.FeatureServices.FindAsync(id);
            if (featureService == null)
            {
                return Json(new { success = false, message = "找不到該服務項目。" });
            }

            featureService.IsActive = isActive;
            _context.Entry(featureService).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            string statusText = isActive ? "啟用" : "停用";
            return Json(new { success = true, message = $"已成功{statusText}此資料。" });
        }
    }
}