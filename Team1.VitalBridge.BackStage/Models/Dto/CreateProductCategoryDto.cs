namespace Team1.VitalBridge.BackStage.Models.Dto
{
    public class CreateProductCategoryDto
    {
        // 這是新增類別的資料類別物件
        public int? FatherId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public int FileId { get; set; } // 圖片檔案ID
    }
}
