namespace Team1.VitalBridge.BackStage.Models.Dto
{
    public class ProductCategoryDto
    {
        // 這是傳送給前端的資料類別物件
        public int Id { get; set; }
        public int? FatherId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        

    }
    

}
