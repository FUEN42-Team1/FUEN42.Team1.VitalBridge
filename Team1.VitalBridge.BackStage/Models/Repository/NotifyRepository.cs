using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Team1.VitalBridge.BackStage.Models.Dto;


namespace Team1.VitalBridge.BackStage.Models.Repository
{
    public class NotifyRepository : INotifyRepository
    {
        private readonly AppDbContext _context;

        public NotifyRepository(AppDbContext context)
        {
            this._context = context;
        }

        public void CreateNotify(CreateNotifyDTO notifyDTO)
        {
            Notify notify = new Notify
            {
                Title = notifyDTO.Title,
                Text = string.IsNullOrEmpty(notifyDTO.Text) ? null : notifyDTO.Text,
                NotifysUrl = string.IsNullOrEmpty(notifyDTO.NotifysUrl) ? null : notifyDTO.NotifysUrl,
                CategoriesId = notifyDTO.CategoriesId,
                SendDate = notifyDTO.SendDate,
                ValidityDate = notifyDTO.ValidityDate
            };
            _context.Notifys.Add(notify);
            _context.SaveChanges();
        }

        public void DeleteNotify(int id)
        {
            var Notify = _context.Notifys.Find(id);
            _context.Notifys.Remove(Notify);
            _context.SaveChanges();
        }

        public List<Notify> GetAllNotify()
        {
            var Notifys = _context.Notifys
                .Include(n => n.Categories)
                .Include(n => n.NotifyUsers)
                .ThenInclude(u => u.User)
                .ToList();
            return Notifys;
        }

        public Notify GetNotifyById(int id)
        {
            var Notify = _context.Notifys
                .Where(n => n.Id == id)
                .Select(n => new Notify{
                    Id=n.Id,
                    Title=n.Title,
                    Text=n.Text,
                    NotifysUrl = n.NotifysUrl,
                    CategoriesId = n.CategoriesId,
                    SendDate = n.SendDate,
                    ValidityDate = n.ValidityDate,
                    Categories =n.Categories,
                    NotifyUsers = n.NotifyUsers.Select(nu => new NotifyUser{
                        NotifyId = nu.NotifyId,
                        UserId = nu.UserId,
                        IsRead = nu.IsRead,
                        User = nu.User
                    }).ToList()
                })
                .FirstOrDefault();

            return Notify;
        }

        public int GetTotalCount()
        {
            return _context.Notifys.Count();
        }

        public void UpdateNotify(int id, Notify newnotify)
        {
            var Notify = _context.Notifys.Find(id);
            Notify.Text = string.IsNullOrEmpty(newnotify.Text) ? null : newnotify.Text;
            Notify.Title = newnotify.Title;
            Notify.SendDate = newnotify.SendDate;
            Notify.ValidityDate = newnotify.ValidityDate;
            Notify.CategoriesId = newnotify.CategoriesId;
            Notify.NotifysUrl = string.IsNullOrEmpty(newnotify.NotifysUrl) ? null : newnotify.NotifysUrl;
            _context.Update(Notify);
            _context.SaveChanges();
        }
    }
}
