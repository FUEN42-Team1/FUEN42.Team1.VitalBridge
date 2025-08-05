using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class ServiceTargetsController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceTargetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ServiceTargets
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchQuery = "")
        {
            // 處理查詢
            var query = _context.ServiceTargets.AsQueryable();
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(st => st.Name.Contains(searchQuery));
            }

            // 處理排序
            query = query.OrderBy(st => st.Id);

            // 計算分頁
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 取得分頁資料
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 將資料和分頁資訊傳給 View
            var result = new PaginatedResult<ServiceTarget>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return View(result);
        }

        // POST: ServiceTargets/Create
        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name")] ServiceTarget serviceTarget)
        {
            if (ModelState.IsValid)
            {
                // 檢查是否有重複的名稱
                var isNameExists = await _context.ServiceTargets.AnyAsync(st => st.Name == serviceTarget.Name);
                if (isNameExists)
                {
                    return Json(new { success = false, message = "服務對象名稱已存在，請使用其他名稱。" });
                }

                // 確保 IsActive 屬性被設定為 true (因為資料庫有預設值，此處是為了明確化)
                serviceTarget.IsActive = true;

                _context.Add(serviceTarget);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "新增成功。", newId = serviceTarget.Id, newName = serviceTarget.Name });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        // POST: ServiceTargets/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,IsActive")] ServiceTarget serviceTarget)
        {
            if (id != serviceTarget.Id)
            {
                return Json(new { success = false, message = "資料不符，無法進行編輯。" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 檢查是否有重複的名稱，但要排除自己本身
                    var isNameExists = await _context.ServiceTargets.AnyAsync(st => st.Name == serviceTarget.Name && st.Id != serviceTarget.Id);
                    if (isNameExists)
                    {
                        return Json(new { success = false, message = "服務對象名稱已存在，請使用其他名稱。" });
                    }

                    _context.Update(serviceTarget);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "編輯成功。" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceTargetExists(serviceTarget.Id))
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

        // POST: ServiceTargets/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, [FromForm] bool isActive)
        {
            var serviceTarget = await _context.ServiceTargets.FindAsync(id);
            if (serviceTarget == null)
            {
                return Json(new { success = false, message = "找不到要變更的資料。" });
            }

            serviceTarget.IsActive = isActive;
            _context.Entry(serviceTarget).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            string statusText = isActive ? "啟用" : "停用";
            return Json(new { success = true, message = "已成功" + statusText + "此資料。" });
        }

        private bool ServiceTargetExists(int id)
        {
            return _context.ServiceTargets.Any(e => e.Id == id);
        }
    }
}