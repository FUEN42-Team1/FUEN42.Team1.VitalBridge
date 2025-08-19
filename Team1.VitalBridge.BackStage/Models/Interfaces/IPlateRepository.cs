using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.Interface
{
    public interface IPlateRepository
    {
        public List<Plate> GetAllPlate();
        public Plate GetPlateById(int id);
        public void CreatePlate(AddBoxViewModel vm);
    }
}
