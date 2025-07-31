using System.ComponentModel.DataAnnotations;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class NotifyViewModel
    {
        public int Id { get; set; }
        [Display(Name = "標題")]
        public string Title { get; set; }
        [Display(Name = "連結")]
        public string? NotifysUrl { get; set; }

        public int CategoriesId { get; set; }
        [Display(Name = "發送時間")]
        public DateTime SendDate { get; set; }
        [Display(Name = "有效時間")]
        public DateTime? ValidityDate { get; set; }
        [Display(Name = "通知類別")]
        public virtual NotifysCategory Categories { get; set; }
        [Display(Name = "通知對象")]
        public virtual ICollection<NotifyUser> NotifyUsers { get; set; } = new List<NotifyUser>();
    }
}
