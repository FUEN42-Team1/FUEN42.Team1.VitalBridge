using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.Frontend.Models.DTOs.ECShop;
using Team1.VitalBridge.Frontend.Services;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        public CategoryApiController(CategoryService categoryService)
        {
            this._categoryService = categoryService;
        }

        /// <summary>
        /// 取得類別完整路徑 (從根類別到指定類別)
        /// </summary>
        /// <param name="id">類別ID</param>
        /// <returns>類別路徑</returns>
        [HttpGet("Path/{id}")]
        public async Task<IActionResult> GetCategoryPath(int id)
        {
            try
            {
                var result = await _categoryService.GetCategoryPathAsync(id);

                if (result == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        IsSuccess = false,
                        Message = "找不到指定的類別或類別未啟用",
                        Data = null
                    });
                }

                return Ok(new ApiResponseDto<CategoryPathResponseDto>
                {
                    IsSuccess = true,
                    Message = "取得類別路徑成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "取得類別路徑時發生錯誤",
                    Error = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// 取得類別階層資訊 (包含父類別和子類別)
        /// </summary>
        /// <param name="id">類別ID</param>
        /// <returns>類別階層資訊</returns>
        [HttpGet("Hierarchy/{id}")]
        public async Task<IActionResult> GetCategoryHierarchy(int id)
        {
            try
            {
                var result = await _categoryService.GetCategoryHierarchyAsync(id);

                if (result == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        IsSuccess = false,
                        Message = "找不到指定的類別或類別未啟用",
                        Data = null
                    });
                }

                return Ok(new ApiResponseDto<CategoryHierarchyResponseDto>
                {
                    IsSuccess = true,
                    Message = "取得類別階層成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "取得類別階層時發生錯誤",
                    Error = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// 取得所有啟用的類別
        /// </summary>
        /// <returns>類別列表</returns>
        [HttpGet("Enable")]
        public async Task<IActionResult> GetEnabledCategories()
        {
            try
            {
                var result = await _categoryService.GetEnabledCategoriesAsync();

                return Ok(new ApiResponseDto<List<CategoryDto>>
                {
                    IsSuccess = true,
                    Message = "取得類別列表成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "取得類別列表時發生錯誤",
                    Error = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// 取得根類別列表
        /// </summary>
        /// <returns>根類別列表</returns>
        [HttpGet("Root")]
        public async Task<IActionResult> GetRootCategories()
        {
            try
            {
                var result = await _categoryService.GetRootCategoriesAsync();

                return Ok(new ApiResponseDto<List<CategoryTreeDto>>
                {
                    IsSuccess = true,
                    Message = "取得根類別成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "取得根類別時發生錯誤",
                    Error = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// 取得指定類別的子類別
        /// </summary>
        /// <param name="id">父類別ID</param>
        /// <returns>子類別列表</returns>
        [HttpGet("{id}/Children")]
        public async Task<IActionResult> GetChildCategories(int id)
        {
            try
            {
                var result = await _categoryService.GetChildCategoriesAsync(id);

                return Ok(new ApiResponseDto<List<CategoryTreeDto>>
                {
                    IsSuccess = true,
                    Message = "取得子類別成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "取得子類別時發生錯誤",
                    Error = ex.Message,
                    Data = null
                });
            }
        }

        /// <summary>
        /// 取得單一類別詳情
        /// </summary>
        /// <param name="id">類別ID</param>
        /// <returns>類別詳情</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var result = await _categoryService.GetCategoryByIdAsync(id);

                if (result == null)
                {
                    return NotFound(new ApiResponseDto<object>
                    {
                        IsSuccess = false,
                        Message = "找不到指定的類別或類別未啟用",
                        Data = null
                    });
                }

                return Ok(new ApiResponseDto<CategoryDto>
                {
                    IsSuccess = true,
                    Message = "取得類別詳情成功",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDto<object>
                {
                    IsSuccess = false,
                    Message = "取得類別詳情時發生錯誤",
                    Error = ex.Message,
                    Data = null
                });
            }
        }



    }
}
