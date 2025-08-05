using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using System.Linq;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class RoomTypesController : Controller
    {
        private readonly AppDbContext _context;

        public RoomTypesController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var data = await _context.RoomTypes
                .OrderBy(c => c.Id)
                .ToListAsync();
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name")] RoomType roomType)
        {
            if (ModelState.IsValid)
            {
                // 檢查是否有重複的名稱
                var isNameExists = await _context.RoomTypes.AnyAsync(rt => rt.Name == roomType.Name);
                if (isNameExists)
                {
                    return Json(new { success = false, message = "房型名稱已存在，請使用其他名稱。" });
                }

                _context.Add(roomType);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "新增成功。", newId = roomType.Id, newName = roomType.Name });
            }
            return Json(new { success = false, message = "資料驗證失敗。" });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] RoomType roomType)
        {
            if (id != roomType.Id)
            {
                return Json(new { success = false, message = "資料不符，無法進行編輯。" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 檢查是否有重複的名稱，但要排除自己本身
                    var isNameExists = await _context.RoomTypes.AnyAsync(rt => rt.Name == roomType.Name && rt.Id != roomType.Id);
                    if (isNameExists)
                    {
                        return Json(new { success = false, message = "房型名稱已存在，請使用其他名稱。" });
                    }

                    _context.Update(roomType);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "編輯成功。" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomTypeExists(roomType.Id))
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

        private bool RoomTypeExists(int id)
        {
            return _context.RoomTypes.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var roomType = await _context.RoomTypes.FindAsync(id);
            if (roomType == null)
            {
                return Json(new { success = false, message = "找不到要刪除的資料。" });
            }

            try
            {
                _context.RoomTypes.Remove(roomType);
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
