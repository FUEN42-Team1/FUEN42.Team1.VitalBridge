using Team1.VitalBridge.Frontend.Models.DTOs.Location;

namespace Team1.VitalBridge.Frontend.Interfaces
{
    public interface ILocationService
    {
        Task<List<CityDto>> GetCitiesAsync();
        Task<List<TownshipDto>> GetTownshipsByCityAsync(int cityId);

        Task<string?> GetTownshipNameByIdAsync(int townshipId);
    }
}
