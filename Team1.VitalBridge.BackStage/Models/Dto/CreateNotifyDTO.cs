using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Team1.VitalBridge.BackStage.Models.EFModels;

namespace Team1.VitalBridge.BackStage.Models.Dto
{
    public class CreateNotifyDTO
    {
        [Required]
        [Column("title")]
        [StringLength(50)]
        public string Title { get; set; }

        [Column("text")]
        [StringLength(500)]
        public string Text { get; set; }

        [Column("notifysUrl")]
        [StringLength(500)]
        public string NotifysUrl { get; set; }

        [Column("categoriesId")]
        public int CategoriesId { get; set; }

        [Column("sendDate", TypeName = "datetime")]
        public DateTime SendDate { get; set; }

        [Column("validityDate", TypeName = "datetime")]
        public DateTime? ValidityDate { get; set; }

    }
}
