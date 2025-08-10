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
		

		// GET api/<ProductCategoryApiController>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// POST api/<ProductCategoryApiController>
		[HttpPost]
		public void Post([FromBody] string value)
		{
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
	}
}
