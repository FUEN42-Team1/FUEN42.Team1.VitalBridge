namespace Team1.VitalBridge.Frontend.Models.ECShop
{
    public class ProductListItemDto
    {
        // 商品列表項目 DTO

        public int Id { get; set; }
        public string Name { get; set; }
        public string Keypoint { get; set; }
        public decimal Price { get; set; }
        public string ImageFileName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ItemNumber { get; set; }  // 商品貨號

    }
}
