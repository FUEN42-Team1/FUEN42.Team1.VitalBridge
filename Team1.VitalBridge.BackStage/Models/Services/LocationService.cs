using Microsoft.AspNetCore.Mvc.Rendering;
using Team1.VitalBridge.BackStage.Models.DTOs;
using Team1.VitalBridge.BackStage.Models.EFModels;
using static Team1.VitalBridge.BackStage.Models.DTOs.LocationDto;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class LocationService
    {
        private readonly AppDbContext _context;
        public LocationService(AppDbContext context) => _context = context;

        public List<CityDto> GetAllCities()
            => _context.Citys
                .OrderBy(c => c.Name)
                .Select(c => new CityDto(c.Id, c.Name))
                .ToList();

        public List<TownshipDto> GetTownshipsByCityId(int? cityId)
            => _context.Townships
                .Where(t => t.CityId == cityId)
                .OrderBy(t => t.Name)
                .Select(t => new TownshipDto(t.Id, t.Name, t.PostalCode))
                .ToList();

        public NameResolveDto ResolveNames(int? cityId, int? townshipId)
        {
            var cityName = cityId.HasValue
                ? _context.Citys.Where(c => c.Id == cityId).Select(c => c.Name).FirstOrDefault()
                : null;

            var townshipName = townshipId.HasValue
                ? _context.Townships.Where(t => t.Id == townshipId).Select(t => t.Name).FirstOrDefault()
                : null;

            return new NameResolveDto(cityName, townshipName);
        }


    }
}
