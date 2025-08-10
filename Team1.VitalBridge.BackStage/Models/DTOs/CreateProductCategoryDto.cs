namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class CreateProductCategoryDto
    {
        // 這是新增類別的資料類別物件
        public int? FatherId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
