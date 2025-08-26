namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class CategoryHierarchyResponseDto
    {
        /// <summary>
        /// 類別階層回應 DTO
        /// </summary>
        public CategoryDto Category { get; set; }
        public List<CategoryDto> CategoryPath { get; set; } = new List<CategoryDto>();
        public CategoryDto ParentCategory { get; set; }
        public List<CategoryDto> Children { get; set; } = new List<CategoryDto>();

    }
}
