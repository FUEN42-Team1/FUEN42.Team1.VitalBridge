// ... (保持所有 using 語句不變)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class ManagerOrganizationsController : Controller
    {
        private readonly AppDbContext _context;

        public ManagerOrganizationsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            var query = _context.Organizations
                .AsNoTracking()
                .Where(o => !o.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.Name.Contains(keyword) || o.Address.Contains(keyword));
            }

            query = query.OrderByDescending(o => o.Id);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 獲取分頁後的項目列表
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new ManagerOrganizationsViewModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Address = o.Address,
                    BedCount = o.BedCount,
                    IsActive = o.IsActive
                })
                .ToListAsync();

            // 為了能在 Index 頁面中直接生成模態視窗，我們需要預先載入所有詳細資訊
            var organizationIds = items.Select(i => i.Id).ToList();
            var detailsList = await _context.Organizations
                .AsNoTracking()
                .Include(o => o.City)
                .Include(o => o.District)
                .Include(o => o.Type)
                .Include(o => o.Institution)
                .Include(o => o.OrganizationFeatureServices)
                .ThenInclude(ofs => ofs.FeatureService)
                .Include(o => o.OrganizationServiceTargets)
                .ThenInclude(ost => ost.ServiceTarget)
                .Include(o => o.OrganizationSubsidyInfos)
                .ThenInclude(osi => osi.SubsidyInfo)
                .Include(o => o.OrganizationRooms)
                .ThenInclude(or => or.RoomType)
                .Where(o => organizationIds.Contains(o.Id)) // 只查詢當前頁面上的機構
                .Select(organization => new ManagerOrganizationDetailsViewModel
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    PhotoUrl = organization.PhotoUrl,
                    CityId = organization.CityId,
                    CityName = organization.City.Name,
                    DistrictId = organization.DistrictId,
                    DistrictName = organization.District.Name,
                    Address = organization.Address,
                    TypeId = organization.TypeId,
                    TypeName = organization.Type.Name,
                    BedCount = organization.BedCount,
                    AgeLimits = organization.AgeLimits,
                    Description = organization.Description,
                    MapUrl = organization.MapUrl,
                    InstitutionName = organization.Institution.Name,
                    IsActive = organization.IsActive,
                    IsDeleted = organization.IsDeleted,
                    SubsidyInfoDescription = organization.OrganizationSubsidyInfos.Select(osi => osi.SubsidyInfo.Description).ToList(),
                    FeatureServiceNames = organization.OrganizationFeatureServices.Select(ofs => ofs.FeatureService.Name).ToList(),
                    ServiceTargetNames = organization.OrganizationServiceTargets.Select(ost => ost.ServiceTarget.Name).ToList(),
                    Rooms = organization.OrganizationRooms.Select(r => new OrganizationRoomViewModel
                    {
                        Id = r.Id,
                        OrganizationId = r.OrganizationId,
                        RoomTypeId = r.RoomTypeId,
                        RoomTypeName = r.RoomType.Name,
                        MonthlyPrice = r.MonthlyPrice,
                        RoomQuantity = r.RoomQuantity,
                        HasDeposit = r.HasDeposit,
                        DepositAmount = r.DepositAmount,
                        DepositMonths = r.DepositMonths
                    }).ToList()
                })
                .ToListAsync();

            var result = new PaginatedResult<ManagerOrganizationsViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            ViewBag.Keyword = keyword;
            ViewBag.DetailsViewModels = detailsList; // 將詳細資訊列表傳遞給 View

            return View(result);
        }




        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return Json(new { success = false, message = "找不到此機構。" });
            }

            try
            {
                organization.IsActive = !organization.IsActive;
                await _context.SaveChangesAsync();
                return Json(new { success = true, organizationName = organization.Name, newIsActive = organization.IsActive });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "操作失敗，請稍後再試。" });
            }
        }
    }
}