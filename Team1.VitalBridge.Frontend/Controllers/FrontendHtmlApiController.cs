using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    /// <summary>
    /// 前台HTML專用API控制器
    /// 提供簡化且完整的API給 org-search.html 使用
    /// </summary>
    [Route("api/frontend-html")]
    public class FrontendHtmlApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FrontendHtmlApiController> _logger;

        public FrontendHtmlApiController(AppDbContext context, ILogger<FrontendHtmlApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 前台HTML專用：簡化的機構搜尋API
        /// 提供完整的圖片URL和簡化的資料結構
        /// </summary>
        /// <param name="keyword">關鍵字</param>
        /// <param name="cityId">城市ID</param>
        /// <param name="districtId">鄉鎮區ID</param>
        /// <param name="organizationTypes">機構類型ID列表</param>
        /// <param name="maxPrice">最高價格</param>
        /// <param name="page">頁碼</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>簡化的機構搜尋結果</returns>
        [HttpGet("search-organizations")]
        public async Task<IActionResult> SearchOrganizations(
            [FromQuery] string? keyword,
            [FromQuery] int? cityId,
            [FromQuery] int? districtId,
            [FromQuery] List<int>? organizationTypes,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("?? 前台HTML搜尋 - 關鍵字: '{Keyword}', 城市: {CityId}, 鄉鎮: {DistrictId}", 
                    keyword ?? "無", cityId, districtId);

                // 建立基本查詢
                var query = _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .AsQueryable();

                // 地區過濾
                if (cityId.HasValue && cityId > 0)
                {
                    query = query.Where(o => o.CityId == cityId.Value);
                    
                    if (districtId.HasValue && districtId > 0)
                    {
                        query = query.Where(o => o.DistrictId == districtId.Value);
                    }
                }

                // 機構類型過濾
                if (organizationTypes != null && organizationTypes.Any())
                {
                    query = query.Where(o => organizationTypes.Contains(o.TypeId));
                }

                // 價格過濾
                if (maxPrice.HasValue && maxPrice > 0)
                {
                    query = query.Where(o => o.OrganizationRooms.Any(r => r.MonthlyPrice <= maxPrice.Value));
                }

                // 關鍵字搜尋
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var searchKeyword = keyword.Trim();
                    query = query.Where(o => 
                        EF.Functions.Like(o.Name, $"%{searchKeyword}%") || 
                        EF.Functions.Like(o.Address, $"%{searchKeyword}%"));
                }

                // 計算總數量
                var totalCount = await query.CountAsync();

                // 查詢詳細資料
                var organizations = await query
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices.Where(ofs => ofs.FeatureService.IsActive))
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Include(o => o.OrganizationRooms)
                    .OrderBy(o => o.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new
                    {
                        id = o.Id,
                        name = o.Name,
                        typeName = o.Type.Name,
                        typeId = o.TypeId,
                        cityName = o.City.Name,
                        districtName = o.District.Name,
                        address = o.Address,
                        bedCount = o.BedCount,
                        ageLimits = o.AgeLimits,
                        description = o.Description,
                        mapUrl = o.MapUrl,
                        // 機構主圖片 - 提供完整URL
                        photoUrl = !string.IsNullOrEmpty(o.PhotoUrl) ? 
                            $"https://localhost:7242/api/UploadFile/GetFile?fileName={o.PhotoUrl}" : null,
                        // 最低價格
                        minPrice = o.OrganizationRooms.Any() ? 
                            o.OrganizationRooms.Min(r => r.MonthlyPrice) : (decimal?)null,
                        // 特色服務 - 提供完整的圖片URL
                        featureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Take(6)
                            .Select(ofs => new
                            {
                                id = ofs.FeatureService.Id,
                                name = ofs.FeatureService.Name,
                                // 提供完整的圖片URL，方便前台直接使用
                                imageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    $"https://localhost:7242/api/UploadFile/GetFile?fileName={ofs.FeatureService.File.FileName}" : null,
                                // 也提供檔案名稱作為備用
                                fileName = ofs.FeatureService.File != null ? ofs.FeatureService.File.FileName : null
                            })
                            .ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("? 前台HTML搜尋完成 - 找到 {Count}/{Total} 筆結果", 
                    organizations.Count, totalCount);

                return Ok(new
                {
                    success = true,
                    message = $"搜尋完成，共找到 {totalCount} 筆機構",
                    data = organizations,
                    pagination = new
                    {
                        totalCount,
                        currentPage = page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasNextPage = page < Math.Ceiling((double)totalCount / pageSize),
                        hasPreviousPage = page > 1
                    },
                    searchInfo = new
                    {
                        keyword,
                        cityId,
                        districtId,
                        organizationTypes,
                        maxPrice,
                        searchTime = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 前台HTML搜尋失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = "搜尋時發生錯誤，請稍後再試",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 前台HTML專用：快速測試API
        /// 專門用於測試 "測試0818" 機構
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet("quick-test")]
        public async Task<IActionResult> QuickTest()
        {
            try
            {
                _logger.LogInformation("?? 執行前台HTML快速測試...");

                // 搜尋測試0818機構
                var testOrganization = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices.Where(ofs => ofs.FeatureService.IsActive))
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Where(o => o.IsActive && !o.IsDeleted && o.Name.Contains("測試0818"))
                    .Select(o => new
                    {
                        id = o.Id,
                        name = o.Name,
                        typeName = o.Type.Name,
                        cityName = o.City.Name,
                        districtName = o.District.Name,
                        address = o.Address,
                        bedCount = o.BedCount,
                        isActive = o.IsActive,
                        isDeleted = o.IsDeleted,
                        // 機構主圖片完整URL
                        photoUrl = !string.IsNullOrEmpty(o.PhotoUrl) ? 
                            $"https://localhost:7242/api/UploadFile/GetFile?fileName={o.PhotoUrl}" : null,
                        // 特色服務及完整圖片URL
                        featureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => new
                            {
                                id = ofs.FeatureService.Id,
                                name = ofs.FeatureService.Name,
                                imageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    $"https://localhost:7242/api/UploadFile/GetFile?fileName={ofs.FeatureService.File.FileName}" : null,
                                fileName = ofs.FeatureService.File != null ? ofs.FeatureService.File.FileName : null
                            })
                            .ToList(),
                        // 測試圖片URL是否正確
                        imageUrlTest = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive && ofs.FeatureService.FileId.HasValue)
                            .Take(1)
                            .Select(ofs => $"https://localhost:7242/api/UploadFile/GetFile?fileName={ofs.FeatureService.File.FileName}")
                            .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (testOrganization == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到測試0818機構",
                        data = (object?)null,
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation("? 測試0818機構查詢成功 - ID: {Id}, 特色服務數: {ServiceCount}", 
                    testOrganization.id, testOrganization.featureServices.Count);

                return Ok(new
                {
                    success = true,
                    message = "測試0818機構查詢成功",
                    data = testOrganization,
                    testInfo = new
                    {
                        hasFeatureServices = testOrganization.featureServices.Any(),
                        featureServiceCount = testOrganization.featureServices.Count,
                        hasImages = testOrganization.featureServices.Any(fs => !string.IsNullOrEmpty(fs.imageUrl)),
                        imageCount = testOrganization.featureServices.Count(fs => !string.IsNullOrEmpty(fs.imageUrl)),
                        sampleImageUrl = testOrganization.imageUrlTest
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? 前台HTML快速測試失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = "快速測試失敗",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 取得城市列表
        /// </summary>
        /// <returns>城市列表</returns>
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            try
            {
                var cities = await _context.Citys
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new { id = c.Id, name = c.Name })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "城市列表查詢成功",
                    data = cities,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢城市列表失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = "查詢城市列表失敗",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 取得指定城市的鄉鎮區列表
        /// </summary>
        /// <param name="cityId">城市ID</param>
        /// <returns>鄉鎮區列表</returns>
        [HttpGet("districts/{cityId}")]
        public async Task<IActionResult> GetDistricts(int cityId)
        {
            try
            {
                var districts = await _context.Townships
                    .AsNoTracking()
                    .Where(d => d.CityId == cityId)
                    .OrderBy(d => d.Name)
                    .Select(d => new { id = d.Id, name = d.Name })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "鄉鎮區列表查詢成功",
                    data = districts,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢鄉鎮區列表失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = "查詢鄉鎮區列表失敗",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 取得機構類型列表
        /// </summary>
        /// <returns>機構類型列表</returns>
        [HttpGet("organization-types")]
        public async Task<IActionResult> GetOrganizationTypes()
        {
            try
            {
                var types = await _context.OrganizationTypes
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .Select(t => new { id = t.Id, name = t.Name })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "機構類型列表查詢成功",
                    data = types,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查詢機構類型列表失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = "查詢機構類型列表失敗",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}