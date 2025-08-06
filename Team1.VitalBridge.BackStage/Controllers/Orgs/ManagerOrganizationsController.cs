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

        // GET: ManagerOrganizations (Index 方法保持不變)
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string keyword = "")
        {
            // ... (程式碼保持不變)
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

            var result = new PaginatedResult<ManagerOrganizationsViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            ViewBag.Keyword = keyword;

            return View(result);
        }

        // GET: ManagerOrganizations/DetailsPartial/5
        // 此方法將用來獲取部分視圖的內容，以在模態視窗中顯示
        public async Task<IActionResult> DetailsPartial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var organization = await _context.Organizations
                .AsNoTracking()
                .Include(o => o.Institution)
                .Include(o => o.City)
                .Include(o => o.District)
                .Include(o => o.Type)
                .Include(o => o.OrganizationSubsidyInfos)
                    .ThenInclude(osi => osi.SubsidyInfo)
                .Include(o => o.OrganizationFeatureServices)
                    .ThenInclude(ofs => ofs.FeatureService)
                .Include(o => o.OrganizationServiceTargets)
                    .ThenInclude(ost => ost.ServiceTarget)
                .Include(o => o.OrganizationRooms)
                    .ThenInclude(or => or.RoomType)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

            if (organization == null)
            {
                return NotFound();
            }

            var viewModel = new ManagerOrganizationDetailsViewModel
            {
                // ... (這裡的 ViewModel 映射邏輯保持不變)
                Id = organization.Id,
                Name = organization.Name,
                PhotoUrl = organization.PhotoUrl,
                BedCount = organization.BedCount,
                Address = organization.Address,
                Description = organization.Description,
                MapUrl = organization.MapUrl,
                AgeLimits = organization.AgeLimits,

                CityId = organization.CityId,
                CityName = organization.City?.Name,
                DistrictId = organization.DistrictId,
                DistrictName = organization.District?.Name,
                TypeId = organization.TypeId,
                TypeName = organization.Type?.Name,

                InstitutionName = organization.Institution?.Name,

                SubsidyInfoNames = organization.OrganizationSubsidyInfos
                    .Select(osi => osi.SubsidyInfo.Description)
                    .ToList(),
                FeatureServiceNames = organization.OrganizationFeatureServices
                    .Select(ofs => ofs.FeatureService.Name)
                    .ToList(),
                ServiceTargetNames = organization.OrganizationServiceTargets
                    .Select(ost => ost.ServiceTarget.Name)
                    .ToList(),

                Rooms = organization.OrganizationRooms
                    .Select(or => new OrganizationRoomViewModel
                    {
                        Id = or.Id,
                        OrganizationId = or.OrganizationId,
                        RoomTypeId = or.RoomTypeId,
                        RoomTypeName = or.RoomType?.Name,
                        MonthlyPrice = or.MonthlyPrice,
                        RoomQuantity = or.RoomQuantity,
                        HasDeposit = or.HasDeposit,
                        DepositAmount = or.DepositAmount,
                        DepositMonths = or.DepositMonths
                    })
                    .ToList(),

                IsActive = organization.IsActive,
                IsDeleted = organization.IsDeleted
            };

            return PartialView("_DetailsPartial", viewModel);
        }

        // POST: Toggle Active Status (這個方法保持不變)
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            // ... (程式碼保持不變)
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