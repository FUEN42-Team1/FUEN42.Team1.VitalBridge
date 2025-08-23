namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class CategoryDto
    {
        // 分類 DTO
        public int Id { get; set; }
        public string Name { get; set; }

        // 新增屬性（用於階層功能）- 使用一致的命名
        public int? FatherId { get; set; }        // 對應 EF Model 的 FatherId
        public string FatherName { get; set; }    // 對應 EF Model 的 Father.Name
        public bool IsActive { get; set; } = true; // 對應 EF Model 的 IsActive
        public int Level { get; set; } = 0;        // 階層等級，0 為根類別

    }
}
