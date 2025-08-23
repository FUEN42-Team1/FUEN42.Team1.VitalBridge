using Team1.VitalBridge.Frontend.Models.DTOs;

namespace Team1.VitalBridge.Frontend.Models.DTOs.ECShop
{
    public class ProductListResponseDto
    {
        // 商品列表回應 DTO
        public List<ProductListItemDto> Products { get; set; } = new List<ProductListItemDto>();    // 商品列表項目 DTO 列表
        public PaginationDto Pagination { get; set; } // 分頁資訊 DTO
        public SearchInfoDto SearchInfo { get; set; } // 搜尋資訊 DTO
    }
}
