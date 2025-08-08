using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using System.Linq;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class OrganizationTypesController : Controller
    {
        private readonly AppDbContext _context;

        public OrganizationTypesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchQuery = "")
        {
            // 確保頁碼和每頁大小為有效值
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;

            // 取得所有組織類型資料的 IQueryable
            var query = _context.OrganizationTypes.AsQueryable();

            // 根據搜尋條件過濾資料
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(ot => ot.Name.Contains(searchQuery));
            }

            // 取得總筆數
            var totalCount = await query.CountAsync();

            // 計算總頁數
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 取得分頁後的資料
            var items = await query
                .OrderBy(ot => ot.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 建立分頁結果物件
            var paginatedResult = new PaginatedResult<OrganizationType>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            // 返回分頁結果物件給 View
            return View(paginatedResult);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name")] OrganizationType organizationType)
        {
            if (ModelState.IsValid)
            {
                // 檢查是否有重複的名稱
                var isNameExists = await _context.OrganizationTypes.AnyAsync(ot => ot.Name == organizationType.Name);
                if (isNameExists)
                {
                    return Json(new { success = false, message = "機構類型名稱已存在，請使用其他名稱。" });
                }

                // 確保 IsActive 屬性被設定為 true
                organizationType.IsActive = true;

                _context.Add(organizationType);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "新增成功。", newId = organizationType.Id, newName = organizationType.Name });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] OrganizationType organizationType)
        {
            if (id != organizationType.Id)
            {
                return Json(new { success = false, message = "資料不符，無法進行編輯。" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 檢查是否有重複的名稱，但要排除自己本身
                    var isNameExists = await _context.OrganizationTypes.AnyAsync(ot => ot.Name == organizationType.Name && ot.Id != organizationType.Id);
                    if (isNameExists)
                    {
                        return Json(new { success = false, message = "機構類型名稱已存在，請使用其他名稱。" });
                    }

                    _context.Update(organizationType);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "編輯成功。" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationTypeExists(organizationType.Id))
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

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, [FromForm] bool isActive)
        {
            var organizationType = await _context.OrganizationTypes.FindAsync(id);
            if (organizationType == null)
            {
                return Json(new { success = false, message = "找不到要變更的資料。" });
            }

            organizationType.IsActive = isActive;
            _context.Entry(organizationType).State = EntityState.Modified; // 明確地告訴 EF Core 物件已被修改
            await _context.SaveChangesAsync();

            string statusText = isActive ? "啟用" : "停用";
            return Json(new { success = true, message = "已成功" + statusText + "此資料。" });
        }

        private bool OrganizationTypeExists(int id)
        {
            return _context.OrganizationTypes.Any(e => e.Id == id);
        }
    }
}
