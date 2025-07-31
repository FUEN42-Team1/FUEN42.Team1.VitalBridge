using Team1.VitalBridge.BackStage.Models.DataTables;
using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.Interface;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;


namespace Team1.VitalBridge.BackStage.Models.Repository
{
    public class NotifyRepository : INotifyRepository
    {
        private readonly AppDbContext _context;

        public NotifyRepository(AppDbContext context)
        {
            this._context = context;
        }

        public void CreateNotify()
        {
            throw new NotImplementedException();
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
            var Notify = _context.Notifys.Find(id);
            return Notify;
        }

        public int GetTotalCount()
        {
            return _context.Notifys.Count();
        }

        public void UpdateNotify(int id, Notify newnotify)
        {
            var Notify = _context.Notifys.Find(id);
            Notify.Text = newnotify.Text;
            Notify.Title = newnotify.Title;
            Notify.SendDate = newnotify.SendDate;
            Notify.ValidityDate = newnotify.ValidityDate;
            Notify.CategoriesId = newnotify.CategoriesId;
            Notify.NotifysUrl = newnotify.NotifysUrl;
            _context.Update(Notify);
            _context.SaveChanges();
        }
    }
}
