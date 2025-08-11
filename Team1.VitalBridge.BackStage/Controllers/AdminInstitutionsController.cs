using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.ViewModels.Admin;

namespace Team1.VitalBridge.BackStage.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminJwtScheme")]
    [Route("Admin/[controller]/[action]")]


    public class AdminInstitutionsController : Controller
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly LocationService _locationService;
        public AdminInstitutionsController(AppDbContext context, IConfiguration configuration, LocationService locationService)
        {
            this._context = context;
            this._configuration = configuration;
            this._locationService = locationService;
        }








        //處理機構法人相關(帳號)
        //而非機構本身服務項目與機構設施
        public async Task<IActionResult> Index()
        {
            //顯示法人清單

            var list = await _context.Institutions
           .AsNoTracking()
           .Include(i => i.City)      // 如果有 City 導覽屬性
           .Include(i => i.Township)  // 如果有 Township 導覽屬性
           .Select(i => new AdminInstitutionListVM
           {
               InstitutionCode = i.InstitutionCode ?? string.Empty,
               InstitutionName = i.Name ?? string.Empty,
               InstitutionEmail = i.Email ?? string.Empty,
               InstitutionPhone = i.Phone ?? string.Empty,

               PrincipalName = i.PrincipalName ?? string.Empty,
               PrincipalPhone = i.PrincipalPhone ?? string.Empty,

               Status = i.Status,
               IsPhysicalCheck = i.IsPhysicalCheck,
               IsBanned = i.IsBanned,

               CityId = i.CityId,
               CityName = i.City != null ? (i.City.Name ?? "-") : "-",
               TownshipId = i.TownshipId,
               TownshipName = i.Township != null ? (i.Township.Name ?? "-") : "-",
               Address = i.Address ?? string.Empty
           })
           .ToListAsync();

            return View(list);
        }


        //詳細
        [HttpGet]
        public async Task<IActionResult> Details(string institutionCode)
        {
            if (string.IsNullOrWhiteSpace(institutionCode))
                return BadRequest();

            var vm = await _context.Institutions
                .AsNoTracking()
                .Include(i => i.City)
                .Include(i => i.Township)
                .Include(i => i.InstitutionAuditImages)
                .Where(i => i.InstitutionCode == institutionCode)
                .Select(i => new AdminInstitutionDetailVM
                {
                    InstitutionCode = i.InstitutionCode ?? string.Empty,
                    InstitutionName = i.Name ?? string.Empty,
                    InstitutionEmail = i.Email ?? string.Empty,
                    InstitutionPhone = i.Phone ?? string.Empty,

                    PrincipalName = i.PrincipalName ?? string.Empty,
                    PrincipalPhone = i.PrincipalPhone ?? string.Empty,

                    Status = i.Status ?? "Pending",

                    IsPhysicalCheck = i.IsPhysicalCheck,
                    IsBanned = i.IsBanned,

                    CityName = i.City != null ? (i.City.Name ?? "-") : "-",
                    TownshipName = i.Township != null ? (i.Township.Name ?? "-") : "-",
                    Address = i.Address ?? string.Empty,

                    //PermitImageUrl = $"/uploads/institutions/{i.InstitutionCode}/permit.jpg"
                    PermitImageUrl = i.InstitutionAuditImages
                        .Select(ai => "/api/UploadFile/GetFile?fileName=" + ai.ImgName)
                        .FirstOrDefault()





                })
                .FirstOrDefaultAsync();

            if (vm == null)
                return NotFound();

            return View(vm);
        }






        //審核
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(string InstitutionCode, string Decision, string? Reason)
        {
            var code = (InstitutionCode ?? "").Trim();
            var decision = (Decision ?? "").Trim();
            var reason = Reason?.Trim();

            if (string.IsNullOrEmpty(code))
            {
                TempData["Error"] = "缺少機構代碼。";
                return RedirectToAction("Index");
            }
            if (decision != "Approved" && decision != "Rejected")
            {
                TempData["Error"] = "審核結果不正確。";
                return RedirectToAction("Details", new { institutionCode = code });
            }
            if (decision == "Rejected" && string.IsNullOrEmpty(reason))
            {
                TempData["Error"] = "駁回請填寫原因。";
                return RedirectToAction("Details", new { institutionCode = code });
            }

            var inst = await _context.Institutions
                .FirstOrDefaultAsync(i => i.InstitutionCode == code);

            if (inst == null)
            {
                TempData["Error"] = "找不到該機構。";
                return RedirectToAction("Index");
            }

            var current = inst.Status ?? "Pending";
            if (!string.Equals(current, "Pending", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = $"目前狀態為 {current}，不可再次審核。";
                return RedirectToAction("Details", new { institutionCode = code });
            }

            // 更新狀態（不寫 RejectReason 到 DB）
            inst.Status = (decision == "Approved") ? "Approved" : "Rejected";
            await _context.SaveChangesAsync();

            // 通知信
            if (!string.IsNullOrWhiteSpace(inst.Email))
            {
                var subject = (decision == "Approved")
                    ? $"【審核通過】{inst.Name} 機構審核結果通知"
                    : $"【審核未通過】{inst.Name} 機構審核結果通知";

                var body = (decision == "Approved")
                    ? $@"您好，{inst.Name}：

                    您提交的機構資料已審核通過。
                    請使用機構後台帳號登入系統開始使用。

                    此致
                    MVC 長照資訊整合網站"
                    : $@"您好，{inst.Name}：

                    很抱歉，您提交的機構資料目前未通過審核。
                    駁回原因：{reason}

                    請依原因修正後再重新送審，謝謝。

                    此致
                    MVC 長照資訊整合網站";

                try
                {
                    // await _emailSender.SendAsync(inst.Email, subject, body);
                    TempData["Success"] = "審核已完成並寄出通知。";
                }
                catch
                {
                    TempData["Warning"] = "審核已完成，但通知郵件寄送失敗。";
                }
            }
            else
            {
                TempData["Warning"] = "審核已完成，但找不到對方 Email，無法寄送通知。";
            }
            Debug.WriteLine($"[Review] 機構 {code} 審核 {decision} 成功，Email: {inst.Email ?? "無"}");
            return RedirectToAction("Details", new { institutionCode = code });
        }



    }
}
