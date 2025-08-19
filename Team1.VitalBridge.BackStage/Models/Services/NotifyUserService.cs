using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.Dto;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using System.Linq.Dynamic.Core;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class NotifyUserService
    {
        private readonly INotifyUserRepository _repository;
        private readonly AppDbContext _context;

        public NotifyUserService(INotifyUserRepository repository,AppDbContext context)
        {
            this._repository = repository;
            this._context = context;
        }

        public void CreateNotifyUser(int notifyId, int userId)
        {
            _repository.CreateNotifyUser(notifyId, userId);
        }

        public void DeleteNotifyUser(int notifyId, int userId)
        {
            _repository.DeleteNotifyUser(notifyId, userId);
        }

        public List<NotifyUsersTableDTO> GetNotifyUsersByPage(DataTableRequest request,int NotifyId)
        {
            var notifyUsers = _context.NotifyUsers
                .Where(nu => nu.NotifyId == NotifyId)
                .Select(nu => nu.UserId)
                .ToList();
            var query = _context.Users.Where(u => !notifyUsers.Contains(u.Id)).Select(u => new
            {
                UserId = u.Id,
                UserName = u.Name,
                UserEmail = u.Email,
                UserRoles = u.UserRoles.Select(ur => ur.Role.Name),
            });

            string keyword = request.Search.Value.ToLower();
            // 篩選條件（如果有）
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                query = query.Where(n => n.UserName != null && n.UserName.ToLower().Contains(keyword) || n.UserEmail != null && n.UserEmail.ToLower().Contains(keyword));
            }

            if (request.Order != null && request.Order.Any())
            {
                var sortColumnIndex = request.Order.First().Column;
                var sortDirection = request.Order.First().Dir;
                string sortColumn = request.Columns[sortColumnIndex].Data;

                query = query.OrderBy($"{sortColumn} {sortDirection}");
            }
            query.ToList();
            return query.Select(n => new NotifyUsersTableDTO
            {
                Id = NotifyId,
                UserName = n.UserName,
                UserId = n.UserId,
                UserEmail = n.UserEmail,
                UserRoles = string.Join(", ", n.UserRoles),
            })
            .ToList();
        }
    }
}
