
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.BackStage.Models.Services;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly LocationService _locationService;
        public LocationsController(LocationService locationService) => _locationService = locationService;

        [HttpGet("cities")]
        public IActionResult GetCities()
            => Ok(_locationService.GetAllCities());

        [HttpGet("cities/{cityId}/townships")]
        public IActionResult GetTownships(int cityId)
            => Ok(_locationService.GetTownshipsByCityId(cityId));

        [HttpGet("resolve")]
        public IActionResult Resolve([FromQuery] int? cityId, [FromQuery] int? townshipId)
            => Ok(_locationService.ResolveNames(cityId, townshipId));
    }
}
