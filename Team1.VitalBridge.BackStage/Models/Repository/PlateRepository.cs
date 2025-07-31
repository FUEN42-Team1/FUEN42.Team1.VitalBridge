using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Models.Repository
{
    public class PlateRepository : IPlateRepository
    {
        private readonly AppDbContext _context;

        public PlateRepository(AppDbContext context)
        {
            this._context = context;
        }

        public void CreatePlate(AddBoxViewModel vm)
        {
            var FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == vm.DefualImageUrl).Id;
            Plate plate = new Plate
            {
                Name = vm.Name,
                Route = vm.Route,
                Location = vm.Location,
                Enable = true,
                DefaultImageClickUrl = vm.DefualImageClickUrl,
                DefaultImageIntroduct = vm.DefualImageIntroduct,
                DefaultImageFileId = FileId
            };
            _context.Plates.Add(plate);
            _context.SaveChanges();
        }

        public Plate GetPlateById(int id)
        {
            Plate plate = _context.Plates
                .Where(p => p.Id == id)
                .Include(p => p.PlateImages.OrderBy(img => img.DisplayOrder))
                .ThenInclude(img=>img.ImageFile)
                .Include(p => p.DefaultImageFile)
                .FirstOrDefault();
            return plate;
        }

        public List<Plate> GetAllPlate()
        {
            var plates = _context.Plates
                .Include(p => p.DefaultImageFile)
                .OrderBy(p => p.Id)
                .ToList();
            return plates;
        }
    }
}
