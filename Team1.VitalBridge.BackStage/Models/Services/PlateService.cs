using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class PlateService
    {
        private readonly IPlateRepository _repository;

        public PlateService(IPlateRepository repository)
        {
            this._repository = repository;
        }

        public void CreatePlate(AddBoxViewModel vm)
        {
            _repository.CreatePlate(vm);
        }

        public Plate GetPlate(int id)
        {
            return _repository.GetPlateById(id);
        }

        public List<Plate> GetPlates()
        {
            return _repository.GetAllPlate(); ;
        }
    }
}
