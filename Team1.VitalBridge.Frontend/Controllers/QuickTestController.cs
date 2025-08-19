using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    /// <summary>
    /// 快速測試控制器 - 專門用於測試0818機構的快速檢查
    /// </summary>
    [Route("api/quick-test")]
    public class QuickTestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuickTestController> _logger;

        public QuickTestController(AppDbContext context, ILogger<QuickTestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 快速檢查測試0818機構
        /// </summary>
        /// <returns>檢查結果</returns>
        [HttpGet("check-test0818")]
        public async Task<IActionResult> CheckTest0818()
        {
            try
            {
                _logger.LogInformation("🔍 開始檢查測試0818機構...");

                // 直接查詢測試0818機構
                var organization = await _context.Organizations
                    .AsNoTracking()
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Where(o => o.Name.Contains("測試0818"))
                    .FirstOrDefaultAsync();

                if (organization == null)
                {
                    _logger.LogWarning("❌ 找不到測試0818機構");
                    return NotFound(new
                    {
                        success = false,
                        message = "找不到測試0818機構",
                        timestamp = DateTime.UtcNow
                    });
                }

                var result = new
                {
                    success = true,
                    message = "找到測試0818機構",
                    data = new
                    {
                        Id = organization.Id,
                        Name = organization.Name,
                        IsActive = organization.IsActive,
                        IsDeleted = organization.IsDeleted,
                        CityName = organization.City?.Name,
                        DistrictName = organization.District?.Name,
                        Address = organization.Address,
                        TypeName = organization.Type?.Name,
                        BedCount = organization.BedCount,
                        WillShowInFrontend = organization.IsActive && !organization.IsDeleted
                    },
                    timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("✅ 測試0818機構檢查完成 - ID: {Id}, IsActive: {IsActive}, IsDeleted: {IsDeleted}", 
                    organization.Id, organization.IsActive, organization.IsDeleted);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 檢查測試0818機構時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "檢查過程中發生錯誤",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 快速搜尋測試
        /// </summary>
        /// <returns>搜尋測試結果</returns>
        [HttpGet("quick-search")]
        public async Task<IActionResult> QuickSearch()
        {
            try
            {
                var keyword = "測試0818";
                _logger.LogInformation("🧪 快速搜尋測試，關鍵字: {Keyword}", keyword);

                // 模擬前台搜尋條件
                var results = await _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .Where(o => EF.Functions.Like(o.Name, $"%{keyword}%") || EF.Functions.Like(o.Address, $"%{keyword}%"))
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Select(o => new
                    {
                        Id = o.Id,
                        Name = o.Name,
                        TypeName = o.Type.Name,
                        CityName = o.City.Name,
                        DistrictName = o.District.Name,
                        Address = o.Address,
                        BedCount = o.BedCount
                    })
                    .ToListAsync();

                _logger.LogInformation("✅ 快速搜尋完成，找到 {Count} 筆結果", results.Count);

                return Ok(new
                {
                    success = true,
                    message = $"搜尋完成，找到 {results.Count} 筆結果",
                    data = new
                    {
                        SearchKeyword = keyword,
                        ResultCount = results.Count,
                        Results = results
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 快速搜尋測試時發生錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "搜尋測試過程中發生錯誤",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// 資料庫連接測試
        /// </summary>
        /// <returns>連接測試結果</returns>
        [HttpGet("database-test")]
        public async Task<IActionResult> DatabaseTest()
        {
            try
            {
                _logger.LogInformation("🔌 測試資料庫連接...");

                var canConnect = await _context.Database.CanConnectAsync();
                var orgCount = await _context.Organizations.CountAsync();

                _logger.LogInformation("✅ 資料庫連接測試完成 - 連接: {CanConnect}, 機構總數: {Count}", canConnect, orgCount);

                return Ok(new
                {
                    success = true,
                    message = "資料庫連接測試完成",
                    data = new
                    {
                        CanConnect = canConnect,
                        OrganizationCount = orgCount,
                        ConnectionString = _context.Database.GetConnectionString()?.Substring(0, 50) + "..."
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ 資料庫連接測試失敗");
                return StatusCode(500, new
                {
                    success = false,
                    message = "資料庫連接測試失敗",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }
    }
}