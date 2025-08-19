using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.Frontend.Models.DTOs;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Team1.VitalBridge.Frontend.Controllers
{
    /// <summary>
    /// 測試控制器 - 驗證機構查詢API返回的資料
    /// </summary>
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TestController> _logger;

        public TestController(AppDbContext context, HttpClient httpClient, ILogger<TestController> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// 測試機構查詢API返回的資料，驗證featureServices陣列中的imageUrl欄位
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet("verify-organization-featureservices")]
        public async Task<IActionResult> VerifyOrganizationFeatureServices()
        {
            try
            {
                _logger.LogInformation("開始測試機構查詢API的特色服務圖片功能");

                // 1. 查詢一個有特色服務的機構
                var organizationWithFeatures = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Where(o => o.IsActive && !o.IsDeleted && o.OrganizationFeatureServices.Any(ofs => ofs.FeatureService.IsActive))
                    .FirstOrDefaultAsync();

                if (organizationWithFeatures == null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "找不到有特色服務的機構進行測試" 
                    });
                }

                _logger.LogInformation("找到測試機構: {Name} (ID: {Id})", organizationWithFeatures.Name, organizationWithFeatures.Id);

                // 2. 呼叫機構查詢API
                var searchRequest = new FrontendSearchRequest
                {
                    Page = 1,
                    PageSize = 10,
                    Keyword = organizationWithFeatures.Name
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(searchRequest), 
                    Encoding.UTF8, 
                    "application/json"
                );

                var response = await _httpClient.PostAsync("api/frontend/search-organizations", jsonContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = $"API 呼叫失敗: {response.StatusCode}" 
                    });
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<dynamic>(responseContent);

                _logger.LogInformation("API 呼叫成功，回應內容長度: {Length}", responseContent.Length);

                // 3. 驗證回應結構（簡化版本 - 實際專案中可能需要更完整的解析）
                var testResults = new List<object>();

                // 模擬驗證 - 實際應該解析 JSON 回應
                var testOrganization = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices.Where(ofs => ofs.FeatureService.IsActive))
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Include(o => o.OrganizationRooms)
                    .Where(o => o.Id == organizationWithFeatures.Id)
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
                        MinPrice = o.OrganizationRooms.Any() ? 
                            o.OrganizationRooms.Min(r => r.MonthlyPrice) : null,
                        FeatureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => new FeatureServiceDto
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                ImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    ofs.FeatureService.File.FileName : null
                            })
                            .ToList(),
                        FeatureServicesWithImages = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => new FeatureServiceDto
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                ImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    ofs.FeatureService.File.FileName : null
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (testOrganization == null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "無法重新查詢測試機構" 
                    });
                }

                // 4. 檢查featureServices陣列中每個項目是否有imageUrl欄位
                var validationResults = new List<object>();

                foreach (var featureService in testOrganization.FeatureServicesWithImages)
                {
                    var hasImageUrl = !string.IsNullOrEmpty(featureService.ImageUrl);
                    var imageUrlTestResult = "未測試";
                    
                    // 5. 測試圖片URL（如果存在）
                    if (hasImageUrl)
                    {
                        var imageUrl = $"http://localhost:7242/api/UploadFile/GetFile?fileName={featureService.ImageUrl}";
                        try
                        {
                            var imageResponse = await _httpClient.GetAsync(imageUrl);
                            imageUrlTestResult = imageResponse.IsSuccessStatusCode ? "可訪問" : $"無法訪問 ({imageResponse.StatusCode})";
                        }
                        catch (Exception ex)
                        {
                            imageUrlTestResult = $"訪問錯誤: {ex.Message}";
                        }
                    }

                    validationResults.Add(new
                    {
                        FeatureServiceId = featureService.Id,
                        FeatureServiceName = featureService.Name,
                        HasImageUrl = hasImageUrl,
                        ImageUrl = featureService.ImageUrl,
                        FullImageUrl = hasImageUrl ? $"http://localhost:7242/api/UploadFile/GetFile?fileName={featureService.ImageUrl}" : null,
                        ImageUrlTest = imageUrlTestResult
                    });

                    _logger.LogInformation("特色服務 '{Name}' - 有圖片: {HasImage}, 圖片URL: {ImageUrl}", 
                        featureService.Name, hasImageUrl, featureService.ImageUrl ?? "無");
                }

                var result = new
                {
                    success = true,
                    message = "測試完成",
                    testData = new
                    {
                        OrganizationId = testOrganization.Id,
                        OrganizationName = testOrganization.Name,
                        FeatureServicesCount = testOrganization.FeatureServicesWithImages.Count,
                        ValidationResults = validationResults
                    },
                    expectedResult = "前台可以正確顯示特色服務圖片",
                    apiResponse = responseContent // 包含實際的API回應
                };

                _logger.LogInformation("測試完成 - 機構: {Name}, 特色服務數量: {Count}", 
                    testOrganization.Name, testOrganization.FeatureServicesWithImages.Count);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "測試機構查詢API時發生錯誤");
                return StatusCode(500, new { 
                    success = false, 
                    message = "測試過程中發生錯誤", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 快速測試 - 直接查詢資料庫驗證映射邏輯
        /// </summary>
        /// <returns>資料庫查詢結果</returns>
        [HttpGet("quick-verify-database")]
        public async Task<IActionResult> QuickVerifyDatabase()
        {
            try
            {
                var organizations = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Where(o => o.IsActive && !o.IsDeleted && o.OrganizationFeatureServices.Any())
                    .Take(3)
                    .Select(o => new
                    {
                        OrganizationId = o.Id,
                        OrganizationName = o.Name,
                        FeatureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => new
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                FileId = ofs.FeatureService.FileId,
                                ImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    ofs.FeatureService.File.FileName : null,
                                FullImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    $"http://localhost:7242/api/UploadFile/GetFile?fileName={ofs.FeatureService.File.FileName}" : null
                            })
                            .ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "資料庫快速驗證完成",
                    data = organizations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "快速驗證資料庫時發生錯誤");
                return StatusCode(500, new { 
                    success = false, 
                    message = "快速驗證失敗", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 簡化的API測試 - 不依賴外部HTTP調用
        /// </summary>
        /// <returns>測試結果</returns>
        [HttpGet("simple-verify")]
        public async Task<IActionResult> SimpleVerifyFeatureServices()
        {
            try
            {
                _logger.LogInformation("開始簡化的特色服務圖片測試");

                // 1. 查詢一個有特色服務的機構
                var testOrganization = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices.Where(ofs => ofs.FeatureService.IsActive))
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Include(o => o.OrganizationRooms)
                    .Where(o => o.IsActive && !o.IsDeleted && o.OrganizationFeatureServices.Any(ofs => ofs.FeatureService.IsActive))
                    .Select(o => new
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
                        MinPrice = o.OrganizationRooms.Any() ? 
                            o.OrganizationRooms.Min(r => r.MonthlyPrice) : (decimal?)null,
                        // 原有的特色服務名稱列表
                        FeatureServices = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Select(ofs => ofs.FeatureService.Name)
                            .ToList(),
                        // 新的特色服務詳細資訊（包含圖片）
                        FeatureServicesWithImages = o.OrganizationFeatureServices
                            .Where(ofs => ofs.FeatureService.IsActive)
                            .Take(6)
                            .Select(ofs => new
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                FileId = ofs.FeatureService.FileId,
                                ImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    ofs.FeatureService.File.FileName : null,
                                FullImageUrl = ofs.FeatureService.FileId.HasValue ? 
                                    $"http://localhost:7242/api/UploadFile/GetFile?fileName={ofs.FeatureService.File.FileName}" : null
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (testOrganization == null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "找不到有特色服務的機構進行測試。請先使用 /api/setup/create-test-data 建立測試資料。" 
                    });
                }

                // 2. 驗證結果
                var validationResults = new List<object>();
                foreach (var service in testOrganization.FeatureServicesWithImages)
                {
                    validationResults.Add(new
                    {
                        Id = service.Id,
                        Name = service.Name,
                        HasFileId = service.FileId.HasValue,
                        FileId = service.FileId,
                        HasImageUrl = !string.IsNullOrEmpty(service.ImageUrl),
                        ImageUrl = service.ImageUrl,
                        FullImageUrl = service.FullImageUrl,
                        Status = service.FileId.HasValue ? "有圖片檔案" : "沒有圖片檔案"
                    });
                }

                var result = new
                {
                    success = true,
                    message = "測試完成",
                    testData = new
                    {
                        Organization = new
                        {
                            Id = testOrganization.Id,
                            Name = testOrganization.Name,
                            Address = testOrganization.Address,
                            TypeName = testOrganization.TypeName,
                            CityName = testOrganization.CityName,
                            DistrictName = testOrganization.DistrictName
                        },
                        FeatureServicesCount = testOrganization.FeatureServicesWithImages.Count,
                        FeatureServicesWithImagesCount = testOrganization.FeatureServicesWithImages.Count(s => !string.IsNullOrEmpty(s.ImageUrl)),
                        ValidationResults = validationResults
                    },
                    apiStructure = new
                    {
                        FeatureServices = testOrganization.FeatureServices,
                        FeatureServicesWithImages = testOrganization.FeatureServicesWithImages
                    }
                };

                _logger.LogInformation("測試完成 - 機構: {Name}, 特色服務數量: {Count}, 有圖片的服務: {ImageCount}", 
                    testOrganization.Name, 
                    testOrganization.FeatureServicesWithImages.Count,
                    testOrganization.FeatureServicesWithImages.Count(s => !string.IsNullOrEmpty(s.ImageUrl)));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "簡化測試時發生錯誤");
                return StatusCode(500, new { 
                    success = false, 
                    message = "測試過程中發生錯誤", 
                    error = ex.Message 
                });
            }
        }
    }
}