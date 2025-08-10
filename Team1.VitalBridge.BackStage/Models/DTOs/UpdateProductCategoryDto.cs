namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class UpdateProductCategoryDto
    {
        // 這是更新類別的資料類別物件
        public int Id { get; set; }
        public int? FatherId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } 
    }
}
