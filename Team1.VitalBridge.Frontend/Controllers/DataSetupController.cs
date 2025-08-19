using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    /// <summary>
    /// 資料準備控制器 - 建立測試用的機構和特色服務資料
    /// </summary>
    [Route("api/setup")]
    public class DataSetupController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DataSetupController> _logger;

        public DataSetupController(AppDbContext context, ILogger<DataSetupController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 建立測試用的機構和特色服務資料
        /// </summary>
        /// <returns>建立結果</returns>
        [HttpPost("create-test-data")]
        public async Task<IActionResult> CreateTestData()
        {
            try
            {
                _logger.LogInformation("開始建立測試資料...");

                // 1. 檢查是否已存在測試機構
                var existingOrg = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.Name == "測試0818");

                Organization testOrg;
                if (existingOrg == null)
                {
                    // 找到城市和區域（假設台南市中西區）
                    var city = await _context.Citys.FirstOrDefaultAsync(c => c.Name.Contains("台南") || c.Name.Contains("臺南"));
                    var district = await _context.Townships.FirstOrDefaultAsync(t => t.Name.Contains("中西"));
                    var orgType = await _context.OrganizationTypes.FirstOrDefaultAsync(ot => ot.IsActive);

                    if (city == null || district == null || orgType == null)
                    {
                        return BadRequest(new { success = false, message = "找不到必要的基礎資料（城市、區域或機構類型）" });
                    }

                    // 建立測試機構
                    testOrg = new Organization
                    {
                        Name = "測試0818",
                        PhotoUrl = null,
                        CityId = city.Id,
                        DistrictId = district.Id,
                        Address = "台南市中西區台南市中西區成功路123號",
                        TypeId = orgType.Id,
                        BedCount = 23,
                        AgeLimits = "測試50以上",
                        Description = "政府補助資訊: 老人補助 (花收、中低收), 身心障礙",
                        MapUrl = "https://maps.google.com",
                        IsRecommended = false,
                        IsCertified = false,
                        IsActive = true,
                        IsDeleted = false
                    };

                    _context.Organizations.Add(testOrg);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("已建立測試機構：{Name} (ID: {Id})", testOrg.Name, testOrg.Id);
                }
                else
                {
                    testOrg = existingOrg;
                    _logger.LogInformation("使用現有測試機構：{Name} (ID: {Id})", testOrg.Name, testOrg.Id);
                }

                // 2. 建立測試特色服務（如果不存在）
                var featureServiceNames = new[]
                {
                    "大倫口腔護", "中風復健護", "安全照顧", "有全家設備",
                    "免外出復健", "沐浴服務", "特殊沐浴設備", "特殊復健設備", "適性管理"
                };

                var createdServices = new List<FeatureService>();
                foreach (var serviceName in featureServiceNames)
                {
                    var existingService = await _context.FeatureServices
                        .FirstOrDefaultAsync(fs => fs.Name == serviceName);

                    if (existingService == null)
                    {
                        var newService = new FeatureService
                        {
                            Name = serviceName,
                            FileId = null, // 暫時沒有圖片
                            IsActive = true
                        };

                        _context.FeatureServices.Add(newService);
                        createdServices.Add(newService);
                    }
                    else
                    {
                        createdServices.Add(existingService);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("已處理 {Count} 個特色服務", createdServices.Count);

                // 3. 建立機構與特色服務的關聯
                var existingRelations = await _context.OrganizationFeatureServices
                    .Where(ofs => ofs.OrganizationId == testOrg.Id)
                    .ToListAsync();

                if (!existingRelations.Any())
                {
                    // 為測試機構添加前6個特色服務
                    var servicesToAdd = createdServices.Take(6).ToList();
                    foreach (var service in servicesToAdd)
                    {
                        var relation = new OrganizationFeatureService
                        {
                            OrganizationId = testOrg.Id,
                            FeatureServiceId = service.Id
                        };
                        _context.OrganizationFeatureServices.Add(relation);
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("已為機構 {OrgName} 添加 {Count} 個特色服務", testOrg.Name, servicesToAdd.Count);
                }

                // 4. 回傳結果
                var result = new
                {
                    success = true,
                    message = "測試資料建立完成",
                    data = new
                    {
                        Organization = new
                        {
                            Id = testOrg.Id,
                            Name = testOrg.Name,
                            Address = testOrg.Address
                        },
                        FeatureServices = createdServices.Select(fs => new
                        {
                            Id = fs.Id,
                            Name = fs.Name,
                            HasImage = fs.FileId.HasValue
                        }).ToList()
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立測試資料時發生錯誤");
                return StatusCode(500, new { 
                    success = false, 
                    message = "建立測試資料失敗", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 檢查目前的測試資料狀態
        /// </summary>
        /// <returns>測試資料狀態</returns>
        [HttpGet("check-test-data")]
        public async Task<IActionResult> CheckTestData()
        {
            try
            {
                var testOrg = await _context.Organizations
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .FirstOrDefaultAsync(o => o.Name == "測試0818");

                if (testOrg == null)
                {
                    return Ok(new { 
                        success = true, 
                        message = "尚未建立測試資料", 
                        hasTestData = false 
                    });
                }

                var result = new
                {
                    success = true,
                    message = "測試資料查詢成功",
                    hasTestData = true,
                    data = new
                    {
                        Organization = new
                        {
                            Id = testOrg.Id,
                            Name = testOrg.Name,
                            Address = testOrg.Address,
                            IsActive = testOrg.IsActive,
                            IsDeleted = testOrg.IsDeleted
                        },
                        FeatureServices = testOrg.OrganizationFeatureServices
                            .Select(ofs => new
                            {
                                Id = ofs.FeatureService.Id,
                                Name = ofs.FeatureService.Name,
                                FileId = ofs.FeatureService.FileId,
                                FileName = ofs.FeatureService.File?.FileName,
                                HasImage = ofs.FeatureService.FileId.HasValue,
                                IsActive = ofs.FeatureService.IsActive
                            })
                            .ToList()
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "檢查測試資料時發生錯誤");
                return StatusCode(500, new { 
                    success = false, 
                    message = "檢查測試資料失敗", 
                    error = ex.Message 
                });
            }
        }
    }
}