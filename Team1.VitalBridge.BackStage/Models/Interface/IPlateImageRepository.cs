using Team1.VitalBridge.BackStage.Models.Dto;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.Interface
{
    public interface IPlateImageRepository
    {
        public PlateImage GetPlateImageById(int id);
        public void CreatePlateImage(CreatePlateImageViewModel vm);

        public int? UpdatePlateImage(UpdatePlateImageDto data);

        public void PlateDisplayOrder(List<int> imageOrder);

        public void DeletePlateImage(int id);
    }
}
