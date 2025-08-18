using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.Frontend.Controllers.Base;
using Team1.VitalBridge.Frontend.Models.DTOs;

namespace Team1.VitalBridge.Frontend.Controllers
{
    /// <summary>
    /// 前端機構搜尋 API 控制器
    /// 高效能唯讀查詢，專為前端 org-search.html 設計
    /// </summary>
    [Route("api/frontend")]
    public class FrontendApiController : BaseApiController
    {
        private readonly AppDbContext _context;

        public FrontendApiController(AppDbContext context, ILogger<FrontendApiController> logger) 
            : base(logger)
        {
            _context = context;
        }

        /// <summary>
        /// 取得所有城市
        /// 使用索引優化的查詢
        /// </summary>
        /// <returns>城市列表</returns>
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            try
            {
                var cities = await _context.Citys
                    .AsNoTracking()
                    .OrderBy(c => c.Name)  // 使用 IX_Cities_Name_Sort 索引
                    .Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync();

                _logger.LogInformation("查詢城市列表完成，共 {Count} 筆", cities.Count);
                return SuccessResponse(cities, "城市列表查詢成功");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "查詢城市列表時發生錯誤");
            }
        }

        /// <summary>
        /// 取得指定城市的鄉鎮區
        /// 使用複合索引優化查詢
        /// </summary>
        /// <param name="cityId">城市ID</param>
        /// <returns>鄉鎮區列表</returns>
        [HttpGet("districts/{cityId}")]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            try
            {
                if (cityId <= 0)
                {
                    return ErrorResponse("城市ID不正確", "INVALID_CITY_ID");
                }

                // 使用 IX_Townships_City_Name 複合索引優化查詢
                var districts = await _context.Townships
                    .AsNoTracking()
                    .Where(d => d.CityId == cityId)  // 索引第一欄位
                    .OrderBy(d => d.Name)            // 索引第二欄位
                    .Select(d => new DistrictDto
                    {
                        Id = d.Id,
                        Name = d.Name
                    })
                    .ToListAsync();

                _logger.LogInformation("查詢城市 {CityId} 的鄉鎮區完成，共 {Count} 筆", cityId, districts.Count);
                return SuccessResponse(districts, "鄉鎮區列表查詢成功");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "查詢鄉鎮區列表時發生錯誤");
            }
        }

        /// <summary>
        /// 取得啟用的機構類型
        /// 使用篩選索引優化查詢
        /// </summary>
        /// <returns>機構類型列表</returns>
        [HttpGet("organization-types")]
        public async Task<IActionResult> GetOrganizationTypes()
        {
            try
            {
                // 使用 IX_OrganizationTypes_Active_Name 篩選索引
                var types = await _context.OrganizationTypes
                    .AsNoTracking()
                    .Where(t => t.IsActive)  // 使用篩選索引
                    .OrderBy(t => t.Name)    // 索引包含的排序欄位
                    .Select(t => new OrganizationTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    })
                    .ToListAsync();

                _logger.LogInformation("查詢機構類型完成，共 {Count} 筆", types.Count);
                return SuccessResponse(types, "機構類型列表查詢成功");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "查詢機構類型列表時發生錯誤");
            }
        }

        /// <summary>
        /// 機構搜尋 (唯讀查詢)
        /// 高效能複合查詢，使用多個優化索引
        /// </summary>
        /// <param name="request">搜尋請求</param>
        /// <returns>機構搜尋結果</returns>
        [HttpPost("search-organizations")]
        public async Task<IActionResult> SearchOrganizations([FromBody] FrontendSearchRequest request)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                // 驗證模型
                var validationResult = ValidateModelState();
                if (validationResult != null) return validationResult;

                // 清除 ChangeTracker 以確保獲取最新資料
                _context.ChangeTracker.Clear();

                // 建立基礎查詢 - 使用 IX_Organizations_Status_Covering 索引
                var query = _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.IsActive && !o.IsDeleted)  // 使用篩選索引條件
                    .AsQueryable();

                // 記錄基礎篩選後的數量
                var baseCount = await query.CountAsync();
                _logger.LogInformation("?? 基礎篩選 (IsActive=true, IsDeleted=false) 後機構數量: {Count}", baseCount);

                // 地區篩選 - 使用 IX_Organizations_Location_Filter 索引
                if (request.CityId.HasValue && request.CityId > 0)
                {
                    query = query.Where(o => o.CityId == request.CityId.Value);
                    var cityCount = await query.CountAsync();
                    _logger.LogInformation("?? 城市篩選 (CityId={CityId}) 後機構數量: {Count}", request.CityId.Value, cityCount);
                    
                    if (request.DistrictId.HasValue && request.DistrictId > 0)
                    {
                        query = query.Where(o => o.DistrictId == request.DistrictId.Value);
                        var districtCount = await query.CountAsync();
                        _logger.LogInformation("??? 鄉鎮區篩選 (DistrictId={DistrictId}) 後機構數量: {Count}", request.DistrictId.Value, districtCount);
                    }
                }

                // 機構類型篩選 - 使用 IX_Organizations_Type_Filter 索引
                if (request.OrganizationTypes.Any())
                {
                    query = query.Where(o => request.OrganizationTypes.Contains(o.TypeId));
                    var typeCount = await query.CountAsync();
                    _logger.LogInformation("?? 機構類型篩選 (Types={Types}) 後機構數量: {Count}", 
                        string.Join(",", request.OrganizationTypes), typeCount);
                }

                // 價格篩選 - 使用 IX_OrganizationRooms_Org_Price 索引
                if (request.MaxPrice.HasValue && request.MaxPrice > 0)
                {
                    query = query.Where(o => o.OrganizationRooms.Any(r => r.MonthlyPrice <= request.MaxPrice.Value));
                    var priceCount = await query.CountAsync();
                    _logger.LogInformation("?? 價格篩選 (MaxPrice<={MaxPrice}) 後機構數量: {Count}", request.MaxPrice.Value, priceCount);
                }

                // 關鍵字搜尋 - 使用 IX_Organizations_Name_Address_Search 索引
                if (!string.IsNullOrWhiteSpace(request.Keyword))
                {
                    var keyword = request.Keyword.Trim();
                    query = query.Where(o => 
                        EF.Functions.Like(o.Name, $"%{keyword}%") || 
                        EF.Functions.Like(o.Address, $"%{keyword}%"));
                    var keywordCount = await query.CountAsync();
                    _logger.LogInformation("?? 關鍵字篩選 (Keyword='{Keyword}') 後機構數量: {Count}", keyword, keywordCount);
                }

                // 計算總筆數（在應用其他 Include 前）
                var totalCount = await query.CountAsync();
                _logger.LogInformation("?? 最終篩選結果總數: {TotalCount}", totalCount);

                // 如果沒有任何結果，記錄詳細的 debug 資訊
                if (totalCount == 0)
                {
                    var debugQuery = _context.Organizations.AsNoTracking();
                    var allCount = await debugQuery.CountAsync();
                    var activeCount = await debugQuery.Where(o => o.IsActive).CountAsync();
                    var notDeletedCount = await debugQuery.Where(o => !o.IsDeleted).CountAsync();
                    var activeNotDeletedCount = await debugQuery.Where(o => o.IsActive && !o.IsDeleted).CountAsync();
                    
                    // 檢查最近修改的機構
                    var recentlyModified = await debugQuery
                        .OrderByDescending(o => o.Id)
                        .Take(5)
                        .Select(o => new { o.Id, o.Name, o.IsActive, o.IsDeleted })
                        .ToListAsync();
                    
                    _logger.LogWarning("?? Debug 資訊 - 總機構數: {All}, 啟用: {Active}, 未刪除: {NotDeleted}, 啟用且未刪除: {ActiveNotDeleted}",
                        allCount, activeCount, notDeletedCount, activeNotDeletedCount);
                    
                    _logger.LogInformation("?? 最近的 5 個機構狀態: {@RecentOrgs}", recentlyModified);
                }

                // 應用 Include 和排序 - 使用 IX_Organizations_Name_Sort 索引
                var organizationsQuery = query
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices.Where(ofs => ofs.FeatureService.IsActive))
                        .ThenInclude(ofs => ofs.FeatureService)
                    .Include(o => o.OrganizationRooms)
                    .OrderBy(o => o.Name)    // 使用排序索引
                    .ThenBy(o => o.Id);      // 確保穩定排序

                // 分頁查詢
                var organizations = await organizationsQuery
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(o => new FrontendOrganizationResult
                    {
                        Id = o.Id,
                        Name = o.Name,
                        TypeName = o.Type.Name,
                        TypeId = o.TypeId,
                        CityName = o.City.Name,
                        DistrictName = o.District.Name,
                        Address = o.Address,
                        BedCount = o.BedCount,
                        AgeLimits = o.AgeLimits,
                        Description = o.Description,
                        MapUrl = o.MapUrl,
                        PhotoUrl = o.PhotoUrl,
                        // 計算最低月租價格 - 使用 IX_OrganizationRooms_Price_Org_MinCalc 索引
                        MinPrice = o.OrganizationRooms.Any() ? 
                            o.OrganizationRooms.Min(r => r.MonthlyPrice) : null,
                        // 取得特色服務名稱列表 - 使用 IX_OrganizationFeatureServices_Org_Feature 索引
                        FeatureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => ofs.FeatureService.Name)
                            .ToList()
                    })
                    .ToListAsync();

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation(
                    "? 機構搜尋完成 - 關鍵字: '{Keyword}', 城市: {CityId}, 鄉鎮: {DistrictId}, 類型: [{Types}], 最高價格: {MaxPrice}, " +
                    "結果: {Count}/{Total}, 頁碼: {Page}/{PageSize}, 耗時: {Duration}ms",
                    request.Keyword ?? "無",
                    request.CityId ?? 0,
                    request.DistrictId ?? 0,
                    string.Join(",", request.OrganizationTypes),
                    request.MaxPrice ?? 0,
                    organizations.Count,
                    totalCount,
                    request.Page,
                    request.PageSize,
                    duration.TotalMilliseconds);

                return PagedSuccessResponse(
                    organizations, 
                    totalCount, 
                    request.Page, 
                    request.PageSize, 
                    $"機構搜尋完成，找到 {totalCount} 筆結果");
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;
                _logger.LogError(ex, "? 機構搜尋失敗，耗時: {Duration}ms", duration.TotalMilliseconds);
                return HandleException(ex, "搜尋機構時發生錯誤");
            }
        }

        /// <summary>
        /// 測試用端點：檢查機構狀態
        /// </summary>
        /// <returns>機構狀態統計</returns>
        [HttpGet("debug/organization-status")]
        public async Task<IActionResult> GetOrganizationStatus()
        {
            try
            {
                var stats = await _context.Organizations
                    .AsNoTracking()
                    .GroupBy(o => new { o.IsActive, o.IsDeleted })
                    .Select(g => new
                    {
                        IsActive = g.Key.IsActive,
                        IsDeleted = g.Key.IsDeleted,
                        Count = g.Count(),
                        Organizations = g.Select(o => new
                        {
                            o.Id,
                            o.Name,
                            o.IsActive,
                            o.IsDeleted,
                            o.CityId,
                            o.TypeId
                        }).ToList()
                    })
                    .ToListAsync();

                var totalCount = await _context.Organizations.CountAsync();
                var activeCount = await _context.Organizations.Where(o => o.IsActive).CountAsync();
                var notDeletedCount = await _context.Organizations.Where(o => !o.IsDeleted).CountAsync();
                var frontendVisibleCount = await _context.Organizations.Where(o => o.IsActive && !o.IsDeleted).CountAsync();

                var result = new
                {
                    TotalOrganizations = totalCount,
                    ActiveOrganizations = activeCount,
                    NotDeletedOrganizations = notDeletedCount,
                    FrontendVisibleOrganizations = frontendVisibleCount,
                    StatusBreakdown = stats,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("?? 機構狀態統計：總數 {Total}, 啟用 {Active}, 未刪除 {NotDeleted}, 前台可見 {Visible}",
                    totalCount, activeCount, notDeletedCount, frontendVisibleCount);

                return SuccessResponse(result, "機構狀態統計查詢成功");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "查詢機構狀態統計時發生錯誤");
            }
        }

        /// <summary>
        /// 測試用端點：檢查特定機構詳細資訊
        /// </summary>
        /// <param name="id">機構ID</param>
        /// <returns>機構詳細資訊</returns>
        [HttpGet("debug/organization/{id}")]
        public async Task<IActionResult> GetOrganizationDetail(int id)
        {
            try
            {
                var organization = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Where(o => o.Id == id)
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.IsActive,
                        o.IsDeleted,
                        o.CityId,
                        CityName = o.City.Name,
                        o.DistrictId,
                        DistrictName = o.District.Name,
                        o.TypeId,
                        TypeName = o.Type.Name,
                        o.Address,
                        o.BedCount,
                        o.AgeLimits,
                        o.Description,
                        WillShowInFrontend = o.IsActive && !o.IsDeleted,
                        LastModified = DateTime.UtcNow
                    })
                    .FirstOrDefaultAsync();

                if (organization == null)
                {
                    return ErrorResponse($"找不到 ID 為 {id} 的機構", "ORGANIZATION_NOT_FOUND");
                }

                _logger.LogInformation("?? 機構 {Id} '{Name}' 詳細資訊：IsActive={IsActive}, IsDeleted={IsDeleted}, 前台可見={Visible}",
                    organization.Id, organization.Name, organization.IsActive, organization.IsDeleted, organization.WillShowInFrontend);

                return SuccessResponse(organization, "機構詳細資訊查詢成功");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "查詢機構詳細資訊時發生錯誤");
            }
        }

        /// <summary>
        /// 測試用端點：強制刷新並檢查機構狀態
        /// </summary>
        /// <returns>刷新後的機構狀態統計</returns>
        [HttpPost("debug/refresh-and-check")]
        public async Task<IActionResult> RefreshAndCheckOrganizations()
        {
            try
            {
                // 強制重新載入 DbContext 並清除快取
                _context.ChangeTracker.Clear();
                
                var stats = await _context.Organizations
                    .AsNoTracking()
                    .GroupBy(o => new { o.IsActive, o.IsDeleted })
                    .Select(g => new
                    {
                        IsActive = g.Key.IsActive,
                        IsDeleted = g.Key.IsDeleted,
                        Count = g.Count(),
                        RecentlyModified = g.OrderByDescending(o => o.Id).Take(5).Select(o => new
                        {
                            o.Id,
                            o.Name,
                            o.IsActive,
                            o.IsDeleted
                        }).ToList()
                    })
                    .ToListAsync();

                var frontendVisibleOrgs = await _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .OrderByDescending(o => o.Id)
                    .Take(10)
                    .Select(o => new
                    {
                        o.Id,
                        o.Name,
                        o.IsActive,
                        o.IsDeleted,
                        o.CityId,
                        o.TypeId
                    })
                    .ToListAsync();

                var result = new
                {
                    RefreshTime = DateTime.UtcNow,
                    StatusBreakdown = stats,
                    FrontendVisibleSample = frontendVisibleOrgs,
                    TotalFrontendVisible = await _context.Organizations.Where(o => o.IsActive && !o.IsDeleted).CountAsync(),
                    Message = "快取已清除，資料已重新載入"
                };

                _logger.LogInformation("?? 強制刷新完成，前台可見機構數量: {Count}", result.TotalFrontendVisible);

                return SuccessResponse(result, "強制刷新和檢查完成");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "強制刷新時發生錯誤");
            }
        }
    }
}