using Microsoft.AspNetCore.SignalR;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Microsoft.AspNetCore.SignalR.Client;

namespace Team1.VitalBridge.BackStage.Models.Repository
{
    public class NotifyUserRepository : INotifyUserRepository
    {
        private readonly AppDbContext _context;

        public NotifyUserRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async void CreateNotifyUser(int notifyId, int userId)
        {
            NotifyUser nu = new NotifyUser
            {
                NotifyId = notifyId,
                UserId = userId,
                IsRead = false,
            };
            _context.NotifyUsers.Add(nu);
            _context.SaveChanges();
            var connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7104/chathub")
            .Build();

            await connection.StartAsync();
            await connection.InvokeAsync("newNotify", userId.ToString(), string.Empty);
        }

        public void DeleteNotifyUser(int notifyId, int userId)
        {
            var nu = _context.NotifyUsers.FirstOrDefault(n => n.UserId == userId && n.NotifyId == notifyId);
            _context.NotifyUsers.Remove(nu);
            _context.SaveChanges();
        }
    }
}
