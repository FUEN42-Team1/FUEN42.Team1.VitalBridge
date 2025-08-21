namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    public class HomepageProductsResponseDto
    {
        public List<ProductHomepageDto> SilverFood { get; set; } = new();     // 銀髮食品
        public List<ProductHomepageDto> HealthFood { get; set; } = new();     // 保健食品  
        public List<ProductHomepageDto> HealthDrink { get; set; } = new();    // 健康飲品
        public List<ProductHomepageDto> HealthCare { get; set; } = new();     // 保健照護
    }
}
