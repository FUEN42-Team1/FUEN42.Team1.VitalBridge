using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.Frontend.Controllers.Base;

namespace Team1.VitalBridge.Frontend.Controllers
{
    /// <summary>
    /// 診斷控制器 - 專門用於檢查系統狀態和資料庫連接
    /// </summary>
    [Route("api/diagnostics")]
    public class DiagnosticsController : BaseApiController
    {
        private readonly AppDbContext _context;

        public DiagnosticsController(AppDbContext context, ILogger<DiagnosticsController> logger) 
            : base(logger)
        {
            _context = context;
        }

        /// <summary>
        /// 檢查資料庫連接狀態
        /// </summary>
        /// <returns>資料庫連接狀態</returns>
        [HttpGet("database-connection")]
        public async Task<IActionResult> CheckDatabaseConnection()
        {
            try
            {
                // 測試資料庫連接
                await _context.Database.CanConnectAsync();
                
                // 測試簡單查詢
                var orgCount = await _context.Organizations.CountAsync();
                var featureServiceCount = await _context.FeatureServices.CountAsync();
                var cityCount = await _context.Citys.CountAsync();
                
                _logger.LogInformation("? 資料庫連接成功 - 機構: {OrgCount}, 特色服務: {ServiceCount}, 城市: {CityCount}", 
                    orgCount, featureServiceCount, cityCount);

                return SuccessResponse(new
                {
                    DatabaseConnected = true,
                    OrganizationCount = orgCount,
                    FeatureServiceCount = featureServiceCount,
                    CityCount = cityCount,
                    ConnectionString = _context.Database.GetConnectionString()?.Substring(0, 50) + "...",
                    ServerTime = DateTime.UtcNow,
                    LocalTime = DateTime.Now
                }, "資料庫連接正常");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 資料庫連接失敗");
                return ErrorResponse($"資料庫連接失敗: {ex.Message}", "DATABASE_CONNECTION_FAILED");
            }
        }

        /// <summary>
        /// 檢查指定機構的詳細狀態
        /// </summary>
        /// <param name="name">機構名稱</param>
        /// <returns>機構詳細狀態</returns>
        [HttpGet("organization-detail")]
        public async Task<IActionResult> CheckOrganizationDetail([FromQuery] string name = "測試0818")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = "測試0818";
                }

                _logger.LogInformation("?? 查詢機構詳細資訊: '{Name}'", name);

                var organization = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Include(o => o.OrganizationRooms)
                    .Where(o => o.Name.Contains(name))
                    .Select(o => new
                    {
                        Id = o.Id,
                        Name = o.Name,
                        IsActive = o.IsActive,
                        IsDeleted = o.IsDeleted,
                        CityId = o.CityId,
                        CityName = o.City.Name,
                        DistrictId = o.DistrictId,
                        DistrictName = o.District.Name,
                        Address = o.Address,
                        TypeId = o.TypeId,
                        TypeName = o.Type.Name,
                        BedCount = o.BedCount,
                        AgeLimits = o.AgeLimits,
                        Description = o.Description,
                        PhotoUrl = o.PhotoUrl,
                        MapUrl = o.MapUrl,
                        MinPrice = o.OrganizationRooms.Any() ? 
                            o.OrganizationRooms.Min(r => r.MonthlyPrice) : (decimal?)null,
                        RoomCount = o.OrganizationRooms.Count(),
                        FeatureServiceCount = o.OrganizationFeatureServices.Count(),
                        FeatureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => new
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                IsActive = ofs.FeatureService.IsActive,
                                FileId = ofs.FeatureService.FileId,
                                FileName = ofs.FeatureService.File != null ? ofs.FeatureService.File.FileName : null,
                                ImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    ofs.FeatureService.File.FileName : null
                            })
                            .ToList(),
                        WillShowInFrontend = o.IsActive && !o.IsDeleted
                    })
                    .FirstOrDefaultAsync();

                if (organization == null)
                {
                    _logger.LogWarning("?? 找不到名稱包含 '{Name}' 的機構", name);
                    
                    // 查詢所有機構名稱以提供參考
                    var allOrgNames = await _context.Organizations
                        .AsNoTracking()
                        .Select(o => o.Name)
                        .Take(10)
                        .ToListAsync();
                    
                    return ErrorResponse($"找不到名稱包含 '{name}' 的機構", "ORGANIZATION_NOT_FOUND");
                }

                _logger.LogInformation("? 找到機構: '{Name}' (ID: {Id}), IsActive: {IsActive}, IsDeleted: {IsDeleted}, 前台可見: {Visible}", 
                    organization.Name, organization.Id, organization.IsActive, organization.IsDeleted, organization.WillShowInFrontend);

                return SuccessResponse(organization, $"機構 '{organization.Name}' 詳細資訊查詢成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 查詢機構詳細資訊時發生錯誤");
                return HandleException(ex, "查詢機構詳細資訊時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查機構統計資訊
        /// </summary>
        /// <returns>機構統計資訊</returns>
        [HttpGet("organization-statistics")]
        public async Task<IActionResult> GetOrganizationStatistics()
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
                        SampleNames = g.Take(3).Select(o => o.Name).ToList()
                    })
                    .ToListAsync();

                var totalCount = await _context.Organizations.CountAsync();
                var frontendVisibleCount = await _context.Organizations
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .CountAsync();

                var featureServiceStats = await _context.FeatureServices
                    .AsNoTracking()
                    .GroupBy(fs => fs.IsActive)
                    .Select(g => new
                    {
                        IsActive = g.Key,
                        Count = g.Count(),
                        WithImages = g.Count(fs => fs.FileId != null)
                    })
                    .ToListAsync();

                _logger.LogInformation("?? 機構統計 - 總數: {Total}, 前台可見: {Visible}", totalCount, frontendVisibleCount);

                return SuccessResponse(new
                {
                    TotalOrganizations = totalCount,
                    FrontendVisibleOrganizations = frontendVisibleCount,
                    OrganizationStatusBreakdown = stats,
                    FeatureServiceStats = featureServiceStats,
                    QueryTime = DateTime.UtcNow
                }, "機構統計資訊查詢成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 查詢機構統計資訊時發生錯誤");
                return HandleException(ex, "查詢機構統計資訊時發生錯誤");
            }
        }

        /// <summary>
        /// 測試前台搜尋 API 查詢
        /// </summary>
        /// <param name="keyword">搜尋關鍵字</param>
        /// <returns>搜尋測試結果</returns>
        [HttpGet("test-search")]
        public async Task<IActionResult> TestSearch([FromQuery] string keyword = "測試0818")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    keyword = "測試0818";
                }

                _logger.LogInformation("?? 測試搜尋功能，關鍵字: '{Keyword}'", keyword);

                // 模擬前台搜尋邏輯
                var query = _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .AsQueryable();

                // 關鍵字搜尋
                query = query.Where(o => 
                    EF.Functions.Like(o.Name, $"%{keyword}%") || 
                    EF.Functions.Like(o.Address, $"%{keyword}%"));

                var totalCount = await query.CountAsync();

                var results = await query
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Include(o => o.OrganizationRooms)
                    .Take(5)
                    .Select(o => new
                    {
                        Id = o.Id,
                        Name = o.Name,
                        TypeName = o.Type.Name,
                        CityName = o.City.Name,
                        DistrictName = o.District.Name,
                        Address = o.Address,
                        BedCount = o.BedCount,
                        MinPrice = o.OrganizationRooms.Any() ? 
                            o.OrganizationRooms.Min(r => r.MonthlyPrice) : null,
                        FeatureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Take(6)
                            .Select(ofs => new
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                ImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    ofs.FeatureService.File.FileName : null
                            })
                            .ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("?? 搜尋結果：關鍵字 '{Keyword}' 找到 {Count} 筆機構", keyword, totalCount);

                return SuccessResponse(new
                {
                    SearchKeyword = keyword,
                    TotalFound = totalCount,
                    SampleResults = results,
                    SearchLogic = new
                    {
                        BaseFilter = "IsActive=true AND IsDeleted=false",
                        KeywordFilter = $"Name LIKE '%{keyword}%' OR Address LIKE '%{keyword}%'",
                        IncludeFeatureServices = true,
                        MaxResultsShown = 5
                    },
                    TestTime = DateTime.UtcNow
                }, $"搜尋測試完成，找到 {totalCount} 筆結果");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 測試搜尋功能時發生錯誤");
                return HandleException(ex, "測試搜尋功能時發生錯誤");
            }
        }

        /// <summary>
        /// 檢查特色服務圖片資料
        /// </summary>
        /// <returns>特色服務圖片統計</returns>
        [HttpGet("feature-service-images")]
        public async Task<IActionResult> CheckFeatureServiceImages()
        {
            try
            {
                var services = await _context.FeatureServices
                    .AsNoTracking()
                    .Include(fs => fs.File)
                    .Where(fs => fs.IsActive)
                    .Select(fs => new
                    {
                        Id = fs.Id,
                        Name = fs.Name,
                        IsActive = fs.IsActive,
                        FileId = fs.FileId,
                        HasFile = fs.FileId != null,
                        FileName = fs.File != null ? fs.File.FileName : null,
                        ImageUrl = fs.FileId.HasValue ? fs.File.FileName : null,
                        FullImageUrl = fs.FileId.HasValue ? 
                            $"https://localhost:7242/api/UploadFile/GetFile?fileName={fs.File.FileName}" : null
                    })
                    .ToListAsync();

                var totalServices = services.Count;
                var servicesWithImages = services.Count(s => s.HasFile);
                var servicesWithoutImages = totalServices - servicesWithImages;

                _logger.LogInformation("??? 特色服務圖片統計 - 總數: {Total}, 有圖片: {WithImages}, 無圖片: {WithoutImages}", 
                    totalServices, servicesWithImages, servicesWithoutImages);

                return SuccessResponse(new
                {
                    TotalServices = totalServices,
                    ServicesWithImages = servicesWithImages,
                    ServicesWithoutImages = servicesWithoutImages,
                    ImageCoveragePercentage = totalServices > 0 ? Math.Round((double)servicesWithImages / totalServices * 100, 2) : 0,
                    Services = services,
                    QueryTime = DateTime.UtcNow
                }, "特色服務圖片資料查詢成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 查詢特色服務圖片資料時發生錯誤");
                return HandleException(ex, "查詢特色服務圖片資料時發生錯誤");
            }
        }

        /// <summary>
        /// 系統健康檢查總覽
        /// </summary>
        /// <returns>系統健康狀態</returns>
        [HttpGet("health-check")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var startTime = DateTime.UtcNow;

                // 檢查資料庫連接
                var canConnect = await _context.Database.CanConnectAsync();
                
                // 基本統計
                var orgCount = await _context.Organizations.CountAsync();
                var activeOrgCount = await _context.Organizations.Where(o => o.IsActive && !o.IsDeleted).CountAsync();
                var featureServiceCount = await _context.FeatureServices.Where(fs => fs.IsActive).CountAsync();
                var cityCount = await _context.Citys.CountAsync();

                // 檢查測試0818機構
                var testOrg = await _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.Name.Contains("測試0818"))
                    .Select(o => new { o.Id, o.Name, o.IsActive, o.IsDeleted })
                    .FirstOrDefaultAsync();

                var testOrgInfo = testOrg != null 
                    ? new
                    {
                        Found = true,
                        Id = testOrg.Id,
                        Name = testOrg.Name,
                        IsActive = testOrg.IsActive,
                        IsDeleted = testOrg.IsDeleted,
                        WillShowInFrontend = testOrg.IsActive && !testOrg.IsDeleted
                    } 
                    : new
                    {
                        Found = false,
                        Id = 0,
                        Name = "未找到",
                        IsActive = false,
                        IsDeleted = true,
                        WillShowInFrontend = false
                    };

                var duration = DateTime.UtcNow - startTime;

                var healthStatus = new
                {
                    Status = canConnect ? "Healthy" : "Unhealthy",
                    DatabaseConnected = canConnect,
                    TotalOrganizations = orgCount,
                    ActiveOrganizations = activeOrgCount,
                    ActiveFeatureServices = featureServiceCount,
                    TotalCities = cityCount,
                    TestOrganization = testOrgInfo,
                    ResponseTimeMs = duration.TotalMilliseconds,
                    CheckTime = DateTime.UtcNow,
                    ServerInfo = new
                    {
                        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                        MachineName = Environment.MachineName,
                        ProcessorCount = Environment.ProcessorCount
                    }
                };

                _logger.LogInformation("?? 系統健康檢查完成 - 狀態: {Status}, 耗時: {Duration}ms", 
                    healthStatus.Status, duration.TotalMilliseconds);

                return Ok(new
                {
                    success = true,
                    message = "系統健康檢查完成",
                    data = healthStatus,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 系統健康檢查時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "系統健康檢查失敗",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}