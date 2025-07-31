using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;

namespace Team1.VitalBridge.BackStage.Models.Interface
{
    public interface INotifyRepository
    {
        public List<Notify> GetAllNotify();
        public Notify GetNotifyById(int id);
        public void CreateNotify();

        public int GetTotalCount();

        public void UpdateNotify(int id, Notify notify);

        public void DeleteNotify(int id);
    }
}
