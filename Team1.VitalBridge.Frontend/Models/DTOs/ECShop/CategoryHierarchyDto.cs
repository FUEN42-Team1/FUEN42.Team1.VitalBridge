namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class CategoryHierarchyDto
    {
        public CategoryDto Category { get; set; }
        public List<CategoryDto> CategoryPath { get; set; } = new List<CategoryDto>();
        public CategoryDto ParentCategory { get; set; }
        public List<CategoryDto> Children { get; set; } = new List<CategoryDto>();

    }
}
