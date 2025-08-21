using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs.Orgs;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers.Orgs
{
    /// <summary>
    /// 機構查詢API控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(AppDbContext context, ILogger<OrganizationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 搜尋機構列表
        /// </summary>
        /// <param name="searchDto">搜尋參數</param>
        /// <returns>分頁的機構列表</returns>
        [HttpPost("search")]
        public async Task<ActionResult<PaginatedResultDto<OrganizationListItemDto>>> SearchOrganizations([FromBody] OrganizationSearchDto searchDto)
        {
            try
            {
                var query = _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                    .Include(o => o.OrganizationRooms)
                        .ThenInclude(or => or.RoomType)
                    .AsQueryable();

                // 關鍵字搜尋
                if (!string.IsNullOrEmpty(searchDto.Keyword))
                {
                    query = query.Where(o => o.Name.Contains(searchDto.Keyword));
                }

                // 縣市篩選
                if (searchDto.CityId.HasValue)
                {
                    query = query.Where(o => o.CityId == searchDto.CityId.Value);
                }

                // 鄉鎮區篩選
                if (searchDto.DistrictId.HasValue)
                {
                    query = query.Where(o => o.DistrictId == searchDto.DistrictId.Value);
                }

                // 機構類型篩選
                if (searchDto.OrganizationTypeIds != null && searchDto.OrganizationTypeIds.Any())
                {
                    query = query.Where(o => searchDto.OrganizationTypeIds.Contains(o.TypeId));
                }

                // 特色服務篩選
                if (searchDto.FeatureServiceIds != null && searchDto.FeatureServiceIds.Any())
                {
                    query = query.Where(o => o.OrganizationFeatureServices
                        .Any(ofs => searchDto.FeatureServiceIds.Contains(ofs.FeatureServiceId)));
                }

                // 價格範圍篩選 (根據房型最低價格)
                if (searchDto.MinPrice.HasValue || searchDto.MaxPrice.HasValue)
                {
                    query = query.Where(o => o.OrganizationRooms.Any(room => 
                        (!searchDto.MinPrice.HasValue || room.MonthlyPrice >= searchDto.MinPrice.Value) &&
                        (!searchDto.MaxPrice.HasValue || room.MonthlyPrice <= searchDto.MaxPrice.Value)
                    ));
                }

                // 計算總筆數
                var totalCount = await query.CountAsync();

                // 分頁查詢
                var organizations = await query
                    .OrderByDescending(o => o.IsRecommended)
                    .ThenByDescending(o => o.IsCertified)
                    .ThenBy(o => o.Name)
                    .Skip((searchDto.Page - 1) * searchDto.PageSize)
                    .Take(searchDto.PageSize)
                    .Select(o => new OrganizationListItemDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        CityName = o.City.Name,
                        DistrictName = o.District.Name,
                        Address = o.Address,
                        BedCount = o.BedCount,
                        PhotoUrl = o.PhotoUrl,
                        TypeName = o.Type.Name,
                        MinMonthlyPrice = o.OrganizationRooms.Any() 
                            ? o.OrganizationRooms.Min(room => room.MonthlyPrice) 
                            : null,
                        IsRecommended = o.IsRecommended,
                        IsCertified = o.IsCertified,
                        FeatureServiceNames = o.OrganizationFeatureServices
                            .Select(ofs => ofs.FeatureService.Name)
                            .ToList()
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);

                var result = new PaginatedResultDto<OrganizationListItemDto>
                {
                    Items = organizations,
                    TotalCount = totalCount,
                    PageNumber = searchDto.Page,
                    PageSize = searchDto.PageSize,
                    TotalPages = totalPages
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋機構時發生錯誤");
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 取得機構詳細資料
        /// </summary>
        /// <param name="id">機構ID</param>
        /// <returns>機構詳細資料</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDetailDto>> GetOrganizationDetail(int id)
        {
            try
            {
                var organization = await _context.Organizations
                    .AsNoTracking()
                    .Where(o => o.Id == id && o.IsActive && !o.IsDeleted)
                    .Include(o => o.City)
                    .Include(o => o.District)
                    .Include(o => o.Type)
                    .Include(o => o.OrganizationFeatureServices)
                        .ThenInclude(ofs => ofs.FeatureService)
                        .ThenInclude(fs => fs.File)
                    .Include(o => o.OrganizationServiceTargets)
                        .ThenInclude(ost => ost.ServiceTarget)
                    .Include(o => o.OrganizationSubsidyInfos)
                        .ThenInclude(osi => osi.SubsidyInfo)
                    .Include(o => o.OrganizationRooms)
                        .ThenInclude(or => or.RoomType)
                    .FirstOrDefaultAsync();

                if (organization == null)
                {
                    return NotFound("找不到指定的機構");
                }

                var result = new OrganizationDetailDto
                {
                    Id = organization.Id,
                    Name = organization.Name,
                    CityName = organization.City.Name,
                    DistrictName = organization.District.Name,
                    Address = organization.Address,
                    BedCount = organization.BedCount,
                    PhotoUrl = organization.PhotoUrl,
                    TypeName = organization.Type.Name,
                    Description = organization.Description,
                    MapUrl = organization.MapUrl,
                    AgeLimits = organization.AgeLimits,
                    IsRecommended = organization.IsRecommended,
                    IsCertified = organization.IsCertified,
                    FeatureServices = organization.OrganizationFeatureServices
                        .Select(ofs => new FeatureServiceDto
                        {
                            Id = ofs.FeatureService.Id,
                            Name = ofs.FeatureService.Name,
                            ImageUrl = ofs.FeatureService.File?.FileName
                        })
                        .ToList(),
                    ServiceTargetNames = organization.OrganizationServiceTargets
                        .Select(ost => ost.ServiceTarget.Name)
                        .ToList(),
                    Rooms = organization.OrganizationRooms
                        .Select(or => new RoomInfoDto
                        {
                            Id = or.Id,
                            RoomTypeName = or.RoomType.Name,
                            MonthlyPrice = or.MonthlyPrice,
                            RoomQuantity = or.RoomQuantity,
                            HasDeposit = or.HasDeposit,
                            DepositAmount = or.DepositAmount,
                            DepositMonths = or.DepositMonths
                        })
                        .ToList(),
                    SubsidyInfoDescriptions = organization.OrganizationSubsidyInfos
                        .Select(osi => osi.SubsidyInfo.Description)
                        .ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得機構詳細資料時發生錯誤，機構ID: {Id}", id);
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 取得縣市列表
        /// </summary>
        /// <returns>縣市列表</returns>
        [HttpGet("cities")]
        public async Task<ActionResult<List<CityDto>>> GetCities()
        {
            try
            {
                var cities = await _context.Citys
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync();

                return Ok(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得縣市列表時發生錯誤");
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 取得鄉鎮區列表
        /// </summary>
        /// <param name="cityId">縣市ID</param>
        /// <returns>鄉鎮區列表</returns>
        [HttpGet("cities/{cityId}/districts")]
        public async Task<ActionResult<List<DistrictDto>>> GetDistricts(int cityId)
        {
            try
            {
                var districts = await _context.Townships
                    .AsNoTracking()
                    .Where(t => t.CityId == cityId)
                    .OrderBy(t => t.Name)
                    .Select(t => new DistrictDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        CityId = t.CityId
                    })
                    .ToListAsync();

                return Ok(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得鄉鎮區列表時發生錯誤，縣市ID: {CityId}", cityId);
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 取得機構類型列表
        /// </summary>
        /// <returns>機構類型列表</returns>
        [HttpGet("organization-types")]
        public async Task<ActionResult<List<OrganizationTypeDto>>> GetOrganizationTypes()
        {
            try
            {
                var types = await _context.OrganizationTypes
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.Name)
                    .Select(t => new OrganizationTypeDto
                    {
                        Id = t.Id,
                        Name = t.Name
                    })
                    .ToListAsync();

                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得機構類型列表時發生錯誤");
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 取得特色服務列表
        /// </summary>
        /// <returns>特色服務列表</returns>
        [HttpGet("feature-services")]
        public async Task<ActionResult<List<FeatureServiceDto>>> GetFeatureServices()
        {
            try
            {
                var featureServices = await _context.FeatureServices
                    .AsNoTracking()
                    .Where(fs => fs.IsActive)
                    .Include(fs => fs.File)
                    .OrderBy(fs => fs.Name)
                    .Select(fs => new FeatureServiceDto
                    {
                        Id = fs.Id,
                        Name = fs.Name,
                        ImageUrl = fs.File != null ? fs.File.FileName : null
                    })
                    .ToListAsync();

                return Ok(featureServices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得特色服務列表時發生錯誤");
                return StatusCode(500, "伺服器錯誤，請稍後再試");
            }
        }
    }
}