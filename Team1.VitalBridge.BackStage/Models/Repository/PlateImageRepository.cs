using Team1.VitalBridge.BackStage.Models.Dto;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using FileStream = Team1.VitalBridge.BackStage.Models.EFModels.FileStream;

namespace Team1.VitalBridge.BackStage.Models.Repository
{
    public class PlateImageRepository : IPlateImageRepository
    {
        private readonly AppDbContext _context;

        public PlateImageRepository(AppDbContext context)
        {
            this._context = context;
        }

        public void CreatePlateImage(CreatePlateImageViewModel vm)
        {
            var FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == vm.ImageUrl).Id;
            PlateImage image = new PlateImage
            {
                PlateId = vm.PlateId,
                Introduct = string.IsNullOrEmpty(vm.Introduct) ? null : vm.Introduct,
                ImageFileId = FileId,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                DisplayOrder = null,
                ClickUrl = string.IsNullOrEmpty(vm.ClickUrl) ? null : vm.ClickUrl,
                ClickNumber = null
            };
            _context.PlateImages.Add(image);
            _context.SaveChanges();
        }

        public void DeletePlateImage(int id)
        {
            var plateImage = _context.PlateImages.Find(id);
            _context.PlateImages.Remove(plateImage);
            _context.SaveChanges();
        }

        public PlateImage GetPlateImageById(int id)
        {
            return _context.PlateImages.Where(p => p.Id == id)
                    .Include(p => p.ImageFile).Select(p => new PlateImage
                    {
                        Id = p.Id,
                        PlateId = p.PlateId,
                        Introduct = p.Introduct,
                        ImageFileId = p.ImageFileId,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        DisplayOrder = p.DisplayOrder,
                        ClickUrl = p.ClickUrl,
                        ClickNumber = p.ClickNumber,
                        ImageFile = new FileStream
                        {
                            Id = p.ImageFile.Id,
                            FileName = p.ImageFile.FileName,
                        }
                    })
                    .FirstOrDefault(); ;
        }

        public void PlateDisplayOrder(List<int> imageOrder)
        {
            for (int i = 0; i < imageOrder.Count; i++)
            {
                var image = _context.PlateImages.Find(imageOrder[i]);
                image.DisplayOrder = i;
            }
            _context.SaveChanges();
        }

        public int? UpdatePlateImage(UpdatePlateImageDto data)
        {
            var FileId = _context.FileStreams.FirstOrDefault(f => f.FileName == data.ImageFileName).Id;
            PlateImage plateImage = _context.PlateImages.Find(data.Id);
            plateImage.ClickUrl = data.ClickUrl;
            plateImage.EndDate = data.EndDate;
            plateImage.StartDate = data.StartDate;
            plateImage.Introduct = data.Introduct;
            plateImage.ImageFileId = FileId;
            _context.Update(plateImage);
            _context.SaveChanges();
            return plateImage.PlateId;
        }
    }
}
