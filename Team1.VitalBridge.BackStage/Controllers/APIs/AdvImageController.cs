using Team1.VitalBridge.BackStage.Models.Dto;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvImageController : ControllerBase
    {
        private readonly PlateImageService _service;

        public AdvImageController(PlateImageService service)
        {
            this._service = service;
        }
        [HttpPut]
        public IActionResult Put([FromBody] List<int> imageOrder)
        {
            _service.PlateDisplayOrder(imageOrder);
            return Ok();
        }
        [HttpPut("update")]
        public IActionResult Update(UpdatePlateImageDto data)
        {
            _service.UpdatePlateImage(data);
            return Ok();
        }
        [HttpDelete]
        public IActionResult Delete([FromForm] int id)
        {
            _service.DeletePlateImage(id);
            return Ok();
        }

        [HttpGet("{id}")]

        public IActionResult Get(int id)
        {
            var data = _service.GetPlateImageById(id);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
    }
}
