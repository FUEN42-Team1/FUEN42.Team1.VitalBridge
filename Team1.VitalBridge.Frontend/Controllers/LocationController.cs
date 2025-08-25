using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs.Location;


namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        // 取得所有縣市
        [HttpGet("cities")]
        public async Task<ActionResult<List<CityDto>>> GetCities()
        {
            var cities = await _locationService.GetCitiesAsync();
            return Ok(cities);
        }

        // 取得某縣市下所有鄉鎮
        [HttpGet("townships")]
        public async Task<ActionResult<List<TownshipDto>>> GetTownships([FromQuery] int cityId)
        {
            var townships = await _locationService.GetTownshipsByCityAsync(cityId);
            return Ok(townships);
        }

        // 取得指定縣市名稱
        [HttpGet("city-name")]
        public async Task<ActionResult<string>> GetCityName([FromQuery] int cityId)
        {
            var cities = await _locationService.GetCitiesAsync();
            var city = cities.FirstOrDefault(c => c.Id == cityId);
            return Ok(city?.Name ?? "");
        }

        // 取得指定鄉鎮名稱
        [HttpGet("township-name")]
        public async Task<ActionResult<string>> GetTownshipName([FromQuery] int townshipId)
        {
            var township = await _locationService.GetTownshipNameByIdAsync(townshipId);
            return Ok(township ?? "");
        }
    }
}
