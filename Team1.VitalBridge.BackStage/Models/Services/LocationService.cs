using Microsoft.AspNetCore.Mvc.Rendering;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class LocationService
    {
        private readonly AppDbContext _context;

        public LocationService(AppDbContext context)
        {
            this._context = context;
        }

        // 給 View 顯示名稱用
        public string? GetCityNameById(int? cityId)
        {
            return _context.Citys
                .Where(c => c.Id == cityId)
                .Select(c => c.Name)
                .FirstOrDefault();
        }

        public string? GetTownshipNameById(int? townshipId)
        {
            return _context.Townships
                .Where(t => t.Id == townshipId)
                .Select(t => t.Name)
                .FirstOrDefault();
        }

        // 給下拉用
        public List<SelectListItem> GetAllCities()
        {
            return _context.Citys
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList();
        }

        public List<SelectListItem> GetTownshipsByCityId(int cityId)
        {
            return _context.Townships
                .Where(t => t.CityId == cityId)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();
        }

        // 給 API AJAX 用
        public List<object> GetTownshipListForApi(int cityId)
        {
            return _context.Townships
                .Where(t => t.CityId == cityId)
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    postalCode = t.PostalCode
                })
                .Cast<object>()
                .ToList();
        }


    }
}
