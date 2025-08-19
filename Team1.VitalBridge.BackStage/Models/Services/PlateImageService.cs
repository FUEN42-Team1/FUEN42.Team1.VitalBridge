using Team1.VitalBridge.BackStage.Models.Dto;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class PlateImageService
    {
        private readonly IPlateImageRepository _repository;

        public PlateImageService(IPlateImageRepository repository)
        {
            this._repository = repository;
        }

        public void CreatePlateImage(CreatePlateImageViewModel vm)
        {
            _repository.CreatePlateImage(vm);
        }
        public void PlateDisplayOrder(List<int> imageOrder)
        {
            _repository.PlateDisplayOrder(imageOrder);
        }

        public PlateImage GetPlateImageById(int id)
        {
            return _repository.GetPlateImageById(id);
        }

        public int? UpdatePlateImage(UpdatePlateImageDto data)
        {
            return _repository.UpdatePlateImage(data);
        }

        public void DeletePlateImage(int id)
        {
            _repository.DeletePlateImage(id);
        }


    }
}
