using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductCategoryApiController : ControllerBase
	{
		private readonly ProductCategoryService _service;

		public ProductCategoryApiController( ProductCategoryService Service)
		{
			_service = Service;
		}

		// 取得所有類別的樹枝狀結構列表
		// GET: api/<ProductCategoryApiController>
		[HttpGet]
		public async Task<ActionResult<List<ProductCategoryDto>>> GetCategoryTreeList()
		{
			try
			{
				var categories = await _service.GetCategoryTreeAsync();
				return Ok(categories);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "取得商品列表失敗", error = ex.Message });
			}
		}



		// 根據ID取得單一類別
		// 提供給編輯表的JS使用
		// GET: api/ProductCategory/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// 新增商品類別資料
		// POST:api/ProductCategoryApi
		[HttpPost]
		public async Task<ActionResult<ProductCategoryDto>> CreateCategory([FromBody] CreateProductCategoryDto createDto)
		{
			try
			{
				var newCategory = await _service.CreateAsync(createDto);
				return CreatedAtAction(nameof(Get), new { id = newCategory.Id }, newCategory);
			}
			catch (ArgumentException ex)
			{
				// 如果是驗證錯誤，返回400 Bad Request
				return BadRequest(new { message = "新增商品類別失敗",  error = ex.Message });
			}
			catch (Exception ex)
			{
				// 其他錯誤返回500 Internal Server Error
				return StatusCode(500, new { message = "新增商品類別失敗", error = ex.Message });
			}
		}

		// PUT api/<ProductCategoryApiController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{


		}

		// DELETE api/<ProductCategoryApiController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}

		// 取得父類別選項
		// GET: api/ProductCategoryApi/ParentOptions
		[HttpGet("ParentOptions")]
	    public async Task<ActionResult<List<ProductCategoryDto>>> GetParentOptions()
		{
			try
			{
				var parentOptions = await _service.GetParentOptionsAsync();
				return Ok(parentOptions);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "取得父類別選項失敗", error = ex.Message });
			}
		}

		/// <summary>
		/// 驗證更新 DTO - 手動驗證取代 DataAnnotations
		/// </summary>

	}
}
