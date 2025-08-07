using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.Services;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationApiController : ControllerBase
    {
        private readonly LocationService _locationService;

        public LocationApiController(LocationService locationService)
        {
            _locationService = locationService;
        }

        // GET: /api/location/townships?cityId=1
        [HttpGet("townships")]
        public IActionResult GetTownships(int cityId)
        {
            var townships = _locationService.GetTownshipListForApi(cityId);
            return Ok(townships);
        }
    }
}
