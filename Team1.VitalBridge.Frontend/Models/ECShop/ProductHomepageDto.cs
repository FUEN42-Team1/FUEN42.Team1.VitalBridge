namespace Team1.VitalBridge.Frontend.Models.ECShop
{
    public class ProductHomepageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Keypoint { get; set; }
        public decimal Price { get; set; }
        public string ImageFileName { get; set; } // 第一張圖片檔名
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

    }
}
