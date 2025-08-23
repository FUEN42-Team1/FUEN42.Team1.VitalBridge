namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class CategoryPathDto
    {
        public int CategoryId { get; set; }
        public List<CategoryDto> CategoryPath { get; set; } = new List<CategoryDto>();
    }
}
