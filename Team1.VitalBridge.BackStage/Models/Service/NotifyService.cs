using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Team1.VitalBridge.BackStage.Models.Dto;

namespace Team1.VitalBridge.BackStage.Models.Service
{
    public class NotifyService
    {
        private readonly INotifyRepository _repository;

        public NotifyService(INotifyRepository repository)
        {
            this._repository = repository;
        }

        public void CreateNotify(CreateNotifyDTO notify)
        {
            _repository.CreateNotify(notify);
        }

        public List<NotifyViewModel> GetAllNotify()
        {
            return _repository.GetAllNotify().Select(n => new NotifyViewModel
            {
                Id = n.Id,
                Title = n.Title,
                NotifysUrl = n.NotifysUrl,
                CategoriesId = n.CategoriesId,
                SendDate = n.SendDate,
                ValidityDate = n.ValidityDate,
                Categories = n.Categories,
                NotifyUsers = n.NotifyUsers
            })
            .ToList();
        }

        public NotifyViewModel GetNotifyById(int id)
        {
            var n = _repository.GetNotifyById(id);

            return new NotifyViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Text = n.Text,
                NotifysUrl = n.NotifysUrl,
                CategoriesId = n.CategoriesId,
                SendDate = n.SendDate,
                ValidityDate = n.ValidityDate,
                Categories = n.Categories,
                NotifyUsers = n.NotifyUsers
            };
        }
        public List<NotifyUserDTO> GetNotifyUserById(int id)
        {
            var users = _repository.GetNotifyById(id).NotifyUsers.Select(n=>new NotifyUserDTO
            {
                Name = n.User.Name,
                Email = n.User.Email,
                Id = n.User.Id,
                IsRead = n.IsRead,
            }).ToList();

            return users;
        }

        public List<NotifyViewModel> GetNotifyByPage(DataTableRequest request)
        {
            var query = _repository.GetAllNotify().AsQueryable();

            // 篩選條件（如果有）
            if (!string.IsNullOrEmpty(request.Search?.Value))
            {
                string keyword = request.Search.Value.ToLower();
                query = query.Where(n => n.Title!=null && n.Title.ToLower().Contains(keyword) || n.Text!=null && n.Text.ToLower().Contains(keyword) || n.Categories.Name!=null && n.Categories.Name.ToLower().Contains(keyword));
            }
            var columnMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["categoriesName"] = "categories.Name",
                ["userCount"] = "NotifyUsers.Count"
            };
            if (request.Order != null && request.Order.Any())
            {
                var sortColumnIndex = request.Order.First().Column;
                var sortDirection = request.Order.First().Dir;
                string sortColumn = request.Columns[sortColumnIndex].Data;
                var mappedColumn = columnMap.ContainsKey(sortColumn)
                ? columnMap[sortColumn]
                : sortColumn;


                query = query.OrderBy($"{mappedColumn} {sortDirection}");
            }

            query.ToList();
            return query.Select(n => new NotifyViewModel
            {
                Id = n.Id,
                Title = n.Title,
                NotifysUrl = n.NotifysUrl,
                CategoriesId = n.CategoriesId,
                SendDate = n.SendDate,
                ValidityDate = n.ValidityDate,
                Categories = n.Categories,
                NotifyUsers = n.NotifyUsers
            })
            .ToList();
        }
        public int GetTotalCount()
        {
            return _repository.GetTotalCount();
        }

        public void UpdateNotify(int id, Notify newnotify)
        {
            _repository.UpdateNotify(id,newnotify);
        }

        public void DeleteNotify(int id)
        {
            _repository.DeleteNotify(id);
        }
    }
}
