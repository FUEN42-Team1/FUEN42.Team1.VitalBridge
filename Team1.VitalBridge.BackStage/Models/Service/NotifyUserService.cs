using Team1.VitalBridge.BackStage.Models.Interface;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class NotifyUserService
    {
        private readonly INotifyUserRepository _repository;

        public NotifyUserService(INotifyUserRepository repository)
        {
            this._repository = repository;
        }

        public void CreateNotifyUser(int notifyId, int userId)
        {
            _repository.CreateNotifyUser(notifyId, userId);
        }

        public void DeleteNotifyUser(int notifyId, int userId)
        {
            _repository.DeleteNotifyUser(notifyId, userId);
        }
    }
}
