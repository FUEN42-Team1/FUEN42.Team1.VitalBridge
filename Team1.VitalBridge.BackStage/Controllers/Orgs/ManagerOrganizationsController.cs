// ... (保持所有 using 語句不變)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Controllers.Orgs
{
    public class ManagerOrganizationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ManagerOrganizationsController> _logger;

        public ManagerOrganizationsController(AppDbContext context, ILogger<ManagerOrganizationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ManagerOrganizations/Create
        public async Task<IActionResult> Create()
        {
            // 準備下拉選單資料 - 只顯示啟用的機構類型
            ViewData["CityId"] = new SelectList(await _context.Citys.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DistrictId"] = new SelectList(await _context.Townships.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
            ViewData["TypeId"] = new SelectList(await _context.OrganizationTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(), "Id", "Name");

            // 多對多關係的選項 - 包含圖片信息
            ViewBag.AllSubsidyInfos = await _context.SubsidyInfos
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Description })
                .ToListAsync();
                
            // 特色服務包含圖片信息
            ViewBag.AllFeatureServices = await _context.FeatureServices
                .Where(fs => fs.IsActive)
                .Include(fs => fs.File)
                .OrderBy(fs => fs.Name)
                .Select(fs => new { 
                    Value = fs.Id.ToString(), 
                    Text = fs.Name, 
                    ImageUrl = fs.File != null ? fs.File.FileName : null 
                })
                .ToListAsync();
                
            ViewBag.AllServiceTargets = await _context.ServiceTargets
                .Where(st => st.IsActive)
                .Select(st => new SelectListItem { Value = st.Id.ToString(), Text = st.Name })
                .ToListAsync();
            ViewBag.AllRoomTypes = await _context.RoomTypes
                .Select(rt => new SelectListItem { Value = rt.Id.ToString(), Text = rt.Name })
                .ToListAsync();

            // 初始化一個空的 ViewModel
            var viewModel = new ManagerOrganizationFormViewModel();
            return View(viewModel);
        }

        // POST: ManagerOrganizations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,PhotoUrl,CityId,DistrictId,Address,TypeId,BedCount,AgeLimits,Description,MapUrl,SelectedSubsidyInfoIds,SelectedFeatureServiceIds,SelectedServiceTargetIds,Rooms")] ManagerOrganizationFormViewModel viewModel)
        {
            // 重新載入下拉選單資料，以便在模型驗證失敗時重新顯示表單
            await LoadDropdownsForCreateEdit();

            if (ModelState.IsValid)
            {
                try
                {
                    // 處理圖片 - 從 PhotoUrl 獲取 FileId (安全方式)
                    int? fileId = null;
                    if (!string.IsNullOrEmpty(viewModel.PhotoUrl))
                    {
                        var file = await _context.FileStreams.FirstOrDefaultAsync(f => f.FileName == viewModel.PhotoUrl);
                        fileId = file?.Id;
                    }

                    // 將 ViewModel 數據映射到 Entity Model
                    var organization = new Organization
                    {
                        Name = viewModel.Name,
                        PhotoUrl = viewModel.PhotoUrl, // 直接存檔案名稱
                        BedCount = viewModel.BedCount,
                        CityId = viewModel.CityId,
                        DistrictId = viewModel.DistrictId,
                        Address = viewModel.Address,
                        TypeId = viewModel.TypeId,
                        Description = viewModel.Description,
                        MapUrl = viewModel.MapUrl,
                        AgeLimits = viewModel.AgeLimits,
                        IsRecommended = false,
                        IsCertified = false,
                        IsActive = true, // 新增時預設為啟用
                        IsDeleted = false,
                        FileId = fileId // 使用安全獲取的 FileId
                    };

                    // 處理多對多關係 (SubsidyInfos)
                    if (viewModel.SelectedSubsidyInfoIds != null && viewModel.SelectedSubsidyInfoIds.Any())
                    {
                        foreach (var subsidyInfoId in viewModel.SelectedSubsidyInfoIds)
                        {
                            organization.OrganizationSubsidyInfos.Add(new OrganizationSubsidyInfo { SubsidyInfoId = subsidyInfoId });
                        }
                    }

                    // 處理多對多關係 (FeatureServices)
                    if (viewModel.SelectedFeatureServiceIds != null && viewModel.SelectedFeatureServiceIds.Any())
                    {
                        foreach (var featureServiceId in viewModel.SelectedFeatureServiceIds)
                        {
                            organization.OrganizationFeatureServices.Add(new OrganizationFeatureService { FeatureServiceId = featureServiceId });
                        }
                    }

                    // 處理多對多關係 (ServiceTargets)
                    if (viewModel.SelectedServiceTargetIds != null && viewModel.SelectedServiceTargetIds.Any())
                    {
                        foreach (var serviceTargetId in viewModel.SelectedServiceTargetIds)
                        {
                            organization.OrganizationServiceTargets.Add(new OrganizationServiceTarget { ServiceTargetId = serviceTargetId });
                        }
                    }

                    // 處理嵌套的房型數據 (OrganizationRooms)
                    if (viewModel.Rooms != null && viewModel.Rooms.Any())
                    {
                        foreach (var roomVm in viewModel.Rooms)
                        {
                            // 驗證房型內部數據
                            var validationContext = new ValidationContext(roomVm);
                            var validationResults = new List<ValidationResult>();
                            bool isValidRoom = Validator.TryValidateObject(roomVm, validationContext, validationResults, true);

                            if (!isValidRoom)
                            {
                                foreach (var validationResult in validationResults)
                                {
                                    foreach (var memberName in validationResult.MemberNames)
                                    {
                                        ModelState.AddModelError($"Rooms[{viewModel.Rooms.IndexOf(roomVm)}].{memberName}", validationResult.ErrorMessage);
                                    }
                                }
                                await LoadDropdownsForCreateEdit();
                                return View(viewModel);
                            }

                            organization.OrganizationRooms.Add(new OrganizationRoom
                            {
                                RoomTypeId = roomVm.RoomTypeId.GetValueOrDefault(),
                                MonthlyPrice = roomVm.MonthlyPrice.GetValueOrDefault(),
                                RoomQuantity = roomVm.RoomQuantity.GetValueOrDefault(),
                                HasDeposit = roomVm.HasDeposit.GetValueOrDefault(),
                                DepositAmount = roomVm.HasDeposit.GetValueOrDefault() ? roomVm.DepositAmount : null,
                                DepositMonths = roomVm.HasDeposit.GetValueOrDefault() ? roomVm.DepositMonths : null
                            });
                        }
                    }

                    _context.Add(organization);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"機構 '{organization.Name}' (ID: {organization.Id}) 成功建立。");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "建立機構時發生錯誤。");
                    ModelState.AddModelError("", "建立機構時發生未預期的錯誤，請重試。");
                    await LoadDropdownsForCreateEdit();
                }
            }

            return View(viewModel);
        }

        // --- API 端點提供下拉選單資料 ---

        // GET: api/cities
        [HttpGet]
        [Route("api/ManagerOrganizations/cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Citys.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).ToListAsync();
            return Ok(cities);
        }

        // GET: api/districts
        [HttpGet]
        [Route("api/ManagerOrganizations/districts")]
        public async Task<IActionResult> GetDistricts(int? cityId)
        {
            if (cityId == null || cityId <= 0)
            {
                return Ok(new List<object>());
            }
            var districts = await _context.Townships.Where(d => d.CityId == cityId).OrderBy(d => d.Name).Select(d => new { d.Id, d.Name }).ToListAsync();
            return Ok(districts);
        }

        // GET: api/organizationtypes
        [HttpGet]
        [Route("api/ManagerOrganizations/organizationtypes")]
        public async Task<IActionResult> GetOrganizationTypes()
        {
            var types = await _context.OrganizationTypes.Where(t => t.IsActive).OrderBy(t => t.Name).Select(t => new { t.Id, t.Name }).ToListAsync();
            return Ok(types);
        }

        // GET: api/subsidyinfos
        [HttpGet]
        [Route("api/ManagerOrganizations/subsidyinfos")]
        public async Task<IActionResult> GetSubsidyInfos()
        {
            var subsidyInfos = await _context.SubsidyInfos.OrderBy(s => s.Description).Select(s => new { s.Id, s.Description }).ToListAsync();
            return Ok(subsidyInfos);
        }

        // GET: api/featureservices
        [HttpGet]
        [Route("api/ManagerOrganizations/featureservices")]
        public async Task<IActionResult> GetFeatureServices()
        {
            var featureServices = await _context.FeatureServices
                .Where(f => f.IsActive)
                .Include(f => f.File) // 包含檔案信息
                .OrderBy(f => f.Name)
                .Select(f => new { 
                    f.Id, 
                    f.Name, 
                    ImageUrl = f.File != null ? f.File.FileName : null // 取得檔案名稱
                })
                .ToListAsync();
            return Ok(featureServices);
        }

        // GET: api/servicetargets
        [HttpGet]
        [Route("api/ManagerOrganizations/servicetargets")]
        public async Task<IActionResult> GetServiceTargets()
        {
            var serviceTargets = await _context.ServiceTargets.Where(st => st.IsActive).OrderBy(st => st.Name).Select(st => new { st.Id, st.Name }).ToListAsync();
            return Ok(serviceTargets);
        }

        // GET: api/roomtypes
        [HttpGet]
        [Route("api/ManagerOrganizations/roomtypes")]
        public async Task<IActionResult> GetRoomTypes()
        {
            var roomTypes = await _context.RoomTypes.OrderBy(rt => rt.Name).Select(rt => new { rt.Id, rt.Name }).ToListAsync();
            return Ok(roomTypes);
        }

        // --- 輔助方法 ---

        private async Task LoadDropdownsForCreateEdit()
        {
            ViewData["CityId"] = new SelectList(await _context.Citys.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DistrictId"] = new SelectList(await _context.Townships.OrderBy(d => d.Name).ToListAsync(), "Id", "Name");
            ViewData["TypeId"] = new SelectList(await _context.OrganizationTypes.Where(t => t.IsActive).OrderBy(t => t.Name).ToListAsync(), "Id", "Name");

            ViewBag.AllSubsidyInfos = await _context.SubsidyInfos
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Description })
                .ToListAsync();
                
            // 特色服務包含圖片信息
            ViewBag.AllFeatureServices = await _context.FeatureServices
                .Where(fs => fs.IsActive)
                .Include(fs => fs.File)
                .OrderBy(fs => fs.Name)
                .Select(fs => new { 
                    Value = fs.Id.ToString(), 
                    Text = fs.Name, 
                    ImageUrl = fs.File != null ? fs.File.FileName : null 
                })
                .ToListAsync();
                
            ViewBag.AllServiceTargets = await _context.ServiceTargets
                .Where(st => st.IsActive)
                .Select(st => new SelectListItem { Value = st.Id.ToString(), Text = st.Name })
                .ToListAsync();
            ViewBag.AllRoomTypes = await _context.RoomTypes
                .Select(rt => new SelectListItem { Value = rt.Id.ToString(), Text = rt.Name })
                .ToListAsync();
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
                //.Include(o => o.Institution) // 新增的資料表的欄位
                .Include(o => o.OrganizationFeatureServices)
                .ThenInclude(ofs => ofs.FeatureService)
                .ThenInclude(fs => fs.File) // 包含特色服務的圖片檔案
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
                    //InstitutionName = organization.Institution.Name, // 新增的資料表的欄位
                    IsActive = organization.IsActive,
                    IsDeleted = organization.IsDeleted,
                    SubsidyInfoDescription = organization.OrganizationSubsidyInfos.Select(osi => osi.SubsidyInfo.Description).ToList(),
                    FeatureServiceNames = organization.OrganizationFeatureServices.Select(ofs => ofs.FeatureService.Name).ToList(),
                    FeatureServices = organization.OrganizationFeatureServices.Select(ofs => new FeatureServiceDetailViewModel
                    {
                        Id = ofs.FeatureService.Id,
                        Name = ofs.FeatureService.Name,
                        ImageUrl = ofs.FeatureService.File != null ? ofs.FeatureService.File.FileName : null
                    }).ToList(),
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
            try
            {
                var organization = await _context.Organizations.FindAsync(id);
                if (organization == null)
                {
                    _logger.LogWarning("嘗試切換不存在的機構狀態，機構 ID: {Id}", id);
                    return Json(new { success = false, message = "找不到此機構。" });
                }

                var oldStatus = organization.IsActive;
                organization.IsActive = !organization.IsActive;
                
                // 記錄詳細的變更資訊
                _logger.LogInformation("🔄 正在切換機構 '{Name}' (ID: {Id}) 的狀態：{OldStatus} → {NewStatus}", 
                    organization.Name, organization.Id, oldStatus, organization.IsActive);
                
                // 明確標記實體已修改
                _context.Entry(organization).State = EntityState.Modified;
                
                var result = await _context.SaveChangesAsync();
                
                if (result > 0)
                {
                    _logger.LogInformation("✅ 機構 '{Name}' (ID: {Id}) 狀態切換成功：{Status} (影響 {Rows} 筆資料)", 
                        organization.Name, organization.Id, organization.IsActive ? "啟用" : "停用", result);
                    
                    // 驗證更新後的狀態 (重新查詢以確認)
                    var verifyOrg = await _context.Organizations
                        .AsNoTracking()
                        .Where(o => o.Id == id)
                        .Select(o => new { o.IsActive, o.IsDeleted })
                        .FirstOrDefaultAsync();
                    
                    _logger.LogInformation("🔍 驗證機構 ID: {Id} 當前狀態：IsActive={IsActive}, IsDeleted={IsDeleted}", 
                        id, verifyOrg?.IsActive, verifyOrg?.IsDeleted);
                    
                    var statusText = organization.IsActive ? "啟用" : "停用";
                    return Json(new { 
                        success = true, 
                        organizationName = organization.Name, 
                        newIsActive = organization.IsActive,
                        message = $"機構 '{organization.Name}' 已{statusText}",
                        verifiedStatus = verifyOrg?.IsActive // 回傳驗證後的狀態
                    });
                }
                else
                {
                    _logger.LogWarning("⚠️ 機構 '{Name}' (ID: {Id}) 狀態切換失敗：SaveChanges 回傳 0", 
                        organization.Name, organization.Id);
                    return Json(new { success = false, message = "資料庫更新失敗，請稍後再試。" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 切換機構狀態時發生錯誤，機構 ID: {Id}", id);
                return Json(new { success = false, message = "操作失敗，請稍後再試。" });
            }
        }

        // GET: ManagerOrganizations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // 載入機構資料及相關資訊
            var organization = await _context.Organizations
                .Include(o => o.OrganizationSubsidyInfos)
                .Include(o => o.OrganizationFeatureServices)
                .Include(o => o.OrganizationServiceTargets)
                .Include(o => o.OrganizationRooms)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

            if (organization == null)
            {
                return NotFound();
            }

            // 安全地獲取 FileId - 檢查 PhotoUrl 是否存在且不為空
            int? imageFileId = null;
            if (!string.IsNullOrEmpty(organization.PhotoUrl))
            {
                var fileRecord = await _context.FileStreams.FirstOrDefaultAsync(f => f.FileName == organization.PhotoUrl);
                imageFileId = fileRecord?.Id;
            }

            // 將實體模型映射到表單 ViewModel
            var viewModel = new ManagerOrganizationFormViewModel
            {
                Id = organization.Id,
                Name = organization.Name,
                PhotoUrl = organization.PhotoUrl,
                CityId = organization.CityId,
                DistrictId = organization.DistrictId,
                Address = organization.Address,
                TypeId = organization.TypeId,
                BedCount = organization.BedCount,
                AgeLimits = organization.AgeLimits,
                Description = organization.Description,
                MapUrl = organization.MapUrl,
                FileId = imageFileId, // 使用安全獲取的 FileId

                // 多對多關係的選中項目
                SelectedSubsidyInfoIds = organization.OrganizationSubsidyInfos.Select(osi => osi.SubsidyInfoId).ToList(),
                SelectedFeatureServiceIds = organization.OrganizationFeatureServices.Select(ofs => ofs.FeatureServiceId).ToList(),
                SelectedServiceTargetIds = organization.OrganizationServiceTargets.Select(ost => ost.ServiceTargetId).ToList(),
                
                // 房型資料
                Rooms = organization.OrganizationRooms.Select(or => new OrganizationRoomViewModel
                {
                    Id = or.Id,
                    OrganizationId = or.OrganizationId,
                    RoomTypeId = or.RoomTypeId,
                    MonthlyPrice = or.MonthlyPrice,
                    RoomQuantity = or.RoomQuantity,
                    HasDeposit = or.HasDeposit,
                    DepositAmount = or.DepositAmount,
                    DepositMonths = or.DepositMonths
                }).ToList()
            };

            // 準備下拉選單資料
            await LoadDropdownsForCreateEdit();
            
            return View(viewModel);
        }

        // POST: ManagerOrganizations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PhotoUrl,CityId,DistrictId,Address,TypeId,BedCount,AgeLimits,Description,MapUrl,SelectedSubsidyInfoIds,SelectedFeatureServiceIds,SelectedServiceTargetIds,Rooms")] ManagerOrganizationFormViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            await LoadDropdownsForCreateEdit();

            if (ModelState.IsValid)
            {
                try
                {
                    // 載入現有的機構資料
                    var organization = await _context.Organizations
                        .Include(o => o.OrganizationSubsidyInfos)
                        .Include(o => o.OrganizationFeatureServices)
                        .Include(o => o.OrganizationServiceTargets)
                        .Include(o => o.OrganizationRooms)
                        .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                    if (organization == null)
                    {
                        return NotFound();
                    }

                    // 更新基本資料
                    organization.Name = viewModel.Name;
                    organization.PhotoUrl = viewModel.PhotoUrl;
                    organization.CityId = viewModel.CityId;
                    organization.DistrictId = viewModel.DistrictId;
                    organization.Address = viewModel.Address;
                    organization.TypeId = viewModel.TypeId;
                    organization.BedCount = viewModel.BedCount;
                    organization.AgeLimits = viewModel.AgeLimits;
                    organization.Description = viewModel.Description;
                    organization.MapUrl = viewModel.MapUrl;
                    organization.FileId = viewModel.FileId;

                    // 更新多對多關係 - 補助資訊
                    _context.OrganizationSubsidyInfos.RemoveRange(organization.OrganizationSubsidyInfos);
                    if (viewModel.SelectedSubsidyInfoIds != null && viewModel.SelectedSubsidyInfoIds.Any())
                    {
                        foreach (var subsidyInfoId in viewModel.SelectedSubsidyInfoIds)
                        {
                            organization.OrganizationSubsidyInfos.Add(new OrganizationSubsidyInfo { OrganizationId = organization.Id, SubsidyInfoId = subsidyInfoId });
                        }
                    }

                    // 更新多對多關係 - 特色服務
                    _context.OrganizationFeatureServices.RemoveRange(organization.OrganizationFeatureServices);
                    if (viewModel.SelectedFeatureServiceIds != null && viewModel.SelectedFeatureServiceIds.Any())
                    {
                        foreach (var featureServiceId in viewModel.SelectedFeatureServiceIds)
                        {
                            organization.OrganizationFeatureServices.Add(new OrganizationFeatureService { OrganizationId = organization.Id, FeatureServiceId = featureServiceId });
                        }
                    }

                    // 更新多對多關係 - 服務對象
                    _context.OrganizationServiceTargets.RemoveRange(organization.OrganizationServiceTargets);
                    if (viewModel.SelectedServiceTargetIds != null && viewModel.SelectedServiceTargetIds.Any())
                    {
                        foreach (var serviceTargetId in viewModel.SelectedServiceTargetIds)
                        {
                            organization.OrganizationServiceTargets.Add(new OrganizationServiceTarget { OrganizationId = organization.Id, ServiceTargetId = serviceTargetId });
                        }
                    }

                    // 更新房型資料
                    _context.OrganizationRooms.RemoveRange(organization.OrganizationRooms);
                    if (viewModel.Rooms != null && viewModel.Rooms.Any())
                    {
                        foreach (var roomVm in viewModel.Rooms)
                        {
                            // 驗證房型內部數據
                            var validationContext = new ValidationContext(roomVm);
                            var validationResults = new List<ValidationResult>();
                            bool isValidRoom = Validator.TryValidateObject(roomVm, validationContext, validationResults, true);

                            if (!isValidRoom)
                            {
                                foreach (var validationResult in validationResults)
                                {
                                    foreach (var memberName in validationResult.MemberNames)
                                    {
                                        ModelState.AddModelError($"Rooms[{viewModel.Rooms.IndexOf(roomVm)}].{memberName}", validationResult.ErrorMessage);
                                    }
                                }
                                return View(viewModel);
                            }

                            organization.OrganizationRooms.Add(new OrganizationRoom
                            {
                                OrganizationId = organization.Id,
                                RoomTypeId = roomVm.RoomTypeId.GetValueOrDefault(),
                                MonthlyPrice = roomVm.MonthlyPrice.GetValueOrDefault(),
                                RoomQuantity = roomVm.RoomQuantity.GetValueOrDefault(),
                                HasDeposit = roomVm.HasDeposit.GetValueOrDefault(),
                                DepositAmount = roomVm.HasDeposit.GetValueOrDefault() ? roomVm.DepositAmount : null,
                                DepositMonths = roomVm.HasDeposit.GetValueOrDefault() ? roomVm.DepositMonths : null
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"機構 '{organization.Name}' (ID: {organization.Id}) 成功更新。");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrganizationExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "更新機構時發生錯誤。");
                    ModelState.AddModelError("", "更新機構時發生未預期的錯誤，請重試。");
                }
            }

            return View(viewModel);
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id && !e.IsDeleted);
        }
    }
}