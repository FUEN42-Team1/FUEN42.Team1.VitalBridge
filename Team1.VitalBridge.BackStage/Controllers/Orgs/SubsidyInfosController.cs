using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using System.Linq;
using System.Threading.Tasks;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class SubsidyInfosController : Controller
    {
        private readonly AppDbContext _context;

        public SubsidyInfosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchQuery = "")
        {
            // 確保頁碼和每頁大小為有效值
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // 取得所有補助清單資料的 IQueryable
            var query = _context.SubsidyInfos.AsQueryable();

            // 根據搜尋條件過濾資料
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(s => s.Description.Contains(searchQuery));
            }

            // 取得總筆數
            var totalCount = await query.CountAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 取得分頁後的資料
            var items = await query
                .OrderBy(s => s.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 建立分頁結果物件
            var paginatedResult = new PaginatedResult<SubsidyInfo>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            // 將分頁結果傳遞給 View
            return View(paginatedResult);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Description")] SubsidyInfo subsidyInfo)
        {
            if (ModelState.IsValid)
            {
                // 檢查是否有重複的名稱
                var isDescriptionExists = await _context.SubsidyInfos.AnyAsync(s => s.Description == subsidyInfo.Description);
                if (isDescriptionExists)
                {
                    return Json(new { success = false, message = "補助描述已存在，請使用其他名稱。" });
                }

                _context.Add(subsidyInfo);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "新增成功。", newId = subsidyInfo.Id, newDescription = subsidyInfo.Description });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Description")] SubsidyInfo subsidyInfo)
        {
            if (id != subsidyInfo.Id)
            {
                return Json(new { success = false, message = "資料不符，無法進行編輯。" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 檢查是否有重複的名稱，但要排除自己本身
                    var isDescriptionExists = await _context.SubsidyInfos.AnyAsync(s => s.Description == subsidyInfo.Description && s.Id != subsidyInfo.Id);
                    if (isDescriptionExists)
                    {
                        return Json(new { success = false, message = "補助描述已存在，請使用其他名稱。" });
                    }

                    _context.Update(subsidyInfo);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "編輯成功。" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubsidyInfoExists(subsidyInfo.Id))
                    {
                        return Json(new { success = false, message = "找不到要編輯的資料。" });
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        private bool SubsidyInfoExists(int id)
        {
            return _context.SubsidyInfos.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var subsidyInfo = await _context.SubsidyInfos.FindAsync(id);
            if (subsidyInfo == null)
            {
                return Json(new { success = false, message = "找不到要刪除的資料。" });
            }

            try
            {
                _context.SubsidyInfos.Remove(subsidyInfo);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "刪除成功。" });
            }
            catch (Exception ex)
            {
                // 在實際應用中，您可能需要記錄這個錯誤
                return Json(new { success = false, message = $"刪除失敗：{ex.Message}" });
            }
        }
    }
}