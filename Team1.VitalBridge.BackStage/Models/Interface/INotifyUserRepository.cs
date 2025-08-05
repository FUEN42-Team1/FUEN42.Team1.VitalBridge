namespace Team1.VitalBridge.BackStage.Models.Interface
{
    public interface INotifyUserRepository
    {
        public void CreateNotifyUser(int notifyId, int userId);
        public void DeleteNotifyUser(int notifyId, int userId);
    }
}
