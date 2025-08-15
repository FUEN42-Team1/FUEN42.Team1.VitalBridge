using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Services;
using Team1.VitalBridge.BackStage.Models.ViewModels.Institutions;

namespace Team1.VitalBridge.BackStage.Controllers.Institutions
{
    public class InstitutionOwnershipController : Controller
    {

        private readonly AppDbContext _context;
        private readonly LocationService _locationService;

        public InstitutionOwnershipController(AppDbContext context, LocationService locationService)
        {
            _context = context;
            _locationService = locationService;
        }



        //機構認領

        public IActionResult Index()
        {
            return View();
        }

        //機構認領列表
        //[HttpGet]
        //public async Task<IActionResult> SearchList(string? q = null, int? cityId = null, int? townshipId = null, bool onlyClaimable = true)
        //{
        //    // 基本過濾：不顯示停權、僅顯示「已核可」的機構（避免去認領駁回/審核中的）
        //    var query = _context.Institutions
        //        .AsNoTracking()
        //        .Where(x => !x.IsBanned && x.Status == "Approved");

        //    // 可選：關鍵字（名稱 / 機構代碼）
        //    if (!string.IsNullOrWhiteSpace(q))
        //    {
        //        var kw = q.Trim();
        //        query = query.Where(x => x.Name.Contains(kw) || x.InstitutionCode.Contains(kw));
        //    }

        //    // 可選：地區過濾
        //    if (cityId.HasValue) query = query.Where(x => x.CityId == cityId.Value);
        //    if (townshipId.HasValue) query = query.Where(x => x.TownshipId == townshipId.Value);

        //    // 先把需要的欄位 + 是否已被認領（EXISTS 子查詢）查出來
        //    var raw = await query
        //        .Select(x => new
        //        {
        //            x.Id,
        //            x.InstitutionCode,
        //            x.Name,
        //            x.CityId,
        //            x.TownshipId,
        //            x.Address,
        //            x.Phone,
        //            x.Status,
        //            x.IsPhysicalCheck,
        //            x.UpdatedAt,
        //            IsClaimed = _context.InstitutionProfiles
        //                .Any(u => u.InstitutionId == x.Id && u.IsResponsible) // 你的關聯表 & 欄位
        //        })
        //        .OrderBy(x => x.Name)
        //        .ToListAsync();

        //    // 預設：只顯示可認領
        //    if (onlyClaimable)
        //        raw = raw.Where(r => !r.IsClaimed).ToList();

        //    // 轉為 VM（把 cityId/townshipId 轉中文名）
        //    var list = raw.Select(r =>
        //    {
        //        var resolved = _locationService.ResolveNames(r.CityId, r.TownshipId);
        //        var fullAddr = $"{resolved.CityName}{resolved.TownshipName}{r.Address}";

        //        return new InstitutionClaimListItemVM
        //        {
        //            InstitutionId = r.Id,
        //            InstitutionCode = r.InstitutionCode,
        //            Name = r.Name,
        //            AddressDisplay = fullAddr,
        //            Status = r.Status,
        //            IsPhysicalCheck = r.IsPhysicalCheck,
        //            IsClaimed = r.IsClaimed,
        //            UpdatedAt = r.UpdatedAt,
        //            Phone = r.Phone
        //        };
        //    }).ToList();

        //    return View(list);
        //}




[HttpGet]
    public async Task<IActionResult> SearchList(string? q)
    {
        // 基本條件：未停權 + 已審核
        var baseQuery = _context.Institutions
            .AsNoTracking()
            .Where(x => !x.IsBanned && x.Status == "Approved");

        // 關鍵字（名稱 / 機構代碼），不分大小寫
        if (!string.IsNullOrWhiteSpace(q))
        {
            var kw = q.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.Like(x.Name, $"%{kw}%") ||
                EF.Functions.Like(x.InstitutionCode, $"%{kw}%"));
        }

        // 固定只顯示「可認領」（尚無負責人）
        baseQuery = baseQuery.Where(x =>
            !_context.InstitutionProfiles.Any(p => p.InstitutionId == x.Id && p.IsResponsible));

        // 取資料（先取必要欄位，再到記憶體組完整地址）
        var raw = await baseQuery
            .Select(x => new
            {
                x.Id,
                x.InstitutionCode,
                x.Name,
                x.CityId,
                x.TownshipId,
                x.Address,
                x.Phone,
                x.Status,
                x.IsPhysicalCheck,
                x.UpdatedAt
            })
            .OrderBy(x => x.Name)
            .Take(100) // 防一次載太多，有需要再做分頁
            .ToListAsync();

        var list = raw.Select(r =>
        {
            var resolved = _locationService.ResolveNames(r.CityId, r.TownshipId);
            var fullAddr = $"{resolved.CityName}{resolved.TownshipName}{r.Address}";
            return new InstitutionClaimListItemVM
            {
                InstitutionId = r.Id,
                InstitutionCode = r.InstitutionCode,
                Name = r.Name,
                AddressDisplay = fullAddr,
                Status = r.Status,
                IsPhysicalCheck = r.IsPhysicalCheck,
                IsClaimed = false, // 此頁固定只列可認領
                UpdatedAt = r.UpdatedAt,
                Phone = r.Phone
            };
        }).ToList();

        ViewBag.Q = q?.Trim() ?? "";
        return View(list);
    }







}
}
