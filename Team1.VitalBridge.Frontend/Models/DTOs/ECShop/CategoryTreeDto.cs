namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class CategoryTreeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? FatherId { get; set; }  // 使用 FatherId 保持一致性
        public bool IsActive { get; set; }
        public bool HasChildren { get; set; }
        public List<CategoryTreeDto> Children { get; set; } = new List<CategoryTreeDto>();
    }
}

