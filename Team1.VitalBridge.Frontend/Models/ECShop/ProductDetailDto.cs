namespace Team1.VitalBridge.Frontend.Models.ECShop
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ItemNumber { get; set; }
        public string Keypoint { get; set; }
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateAt { get; set; }

        // 圖片列表
        public List<ProductImageDto> Images { get; set; }

        // 分類列表
        public List<CategoryDto> Categories { get; set; }

    }
}
