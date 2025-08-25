using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs.Location;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Models.Services
{
    public class LocationService : ILocationService
    {
        private readonly AppDbContext _db;

        public LocationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<CityDto>> GetCitiesAsync()
        {
            return await _db.Citys
                .AsNoTracking()
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<List<TownshipDto>> GetTownshipsByCityAsync(int cityId)
        {
            return await _db.Townships
                .AsNoTracking()
                .Where(t => t.CityId == cityId)
                .Select(t => new TownshipDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    CityId = t.CityId
                })
                .ToListAsync();
        }

        public async Task<string?> GetTownshipNameByIdAsync(int townshipId)
        {
            return await _db.Townships
                .Where(t => t.Id == townshipId)
                .Select(t => t.Name)
                .FirstOrDefaultAsync();
        }
    }
}
