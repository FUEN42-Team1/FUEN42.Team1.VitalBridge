using Team1.VitalBridge.BackStage.Models.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _service;

        public CategoryController(CategoryService service)
        {
            this._service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var Category = _service.getAllCategories();
            return Ok(Category);
        }

        [HttpGet("Enable")]
        public IActionResult GetEnable()
        {
            var Category = _service.getAllCategories().Where(p=>p.Enable==true);
            return Ok(Category);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var Category = _service.getCategoryById(id);

            return Ok(Category);
        }

        [HttpDelete]
        public IActionResult Delete([FromForm] int id)
        {
            _service.deleteCategoryById(id);
            return Ok();
        }

        [HttpPut("PutEnable")]
        public IActionResult PutEnable([FromForm] int id, [FromForm] bool enable)
        {

            _service.setEnable(id, enable);
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromForm] int id, [FromForm] string name)
        {
            _service.setName(id, name);
            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromForm] string name)
        {
            _service.CreateCategory(name);
            return Ok();
        }
    }
}
