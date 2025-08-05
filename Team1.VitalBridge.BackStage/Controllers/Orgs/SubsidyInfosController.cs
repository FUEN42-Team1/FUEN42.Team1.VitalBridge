using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using System.Linq;
using System.Threading.Tasks;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class SubsidyInfosController : Controller
    {
        private readonly AppDbContext _context;

        public SubsidyInfosController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 從資料庫中取得所有的 SubsidyInfo 資料
            var data = await _context.SubsidyInfos
                .OrderBy(s => s.Id)
                .ToListAsync();

            // 將資料傳遞給 View
            return View(data);
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
