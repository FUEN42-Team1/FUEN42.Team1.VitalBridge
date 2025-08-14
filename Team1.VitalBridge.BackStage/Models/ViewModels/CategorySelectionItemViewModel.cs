namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class CategorySelectionItemViewModel
    {
        // 這是要用於前端選擇類別的項目
        public int Id { get; set; }
        public string Name { get; set; }
        public int? FatherId { get; set; }
        public bool IsSelectable { get; set; } // 是否可以選擇
        public int Level { get; set; } // 層級：0=父類別，1=子類別

    }
}
