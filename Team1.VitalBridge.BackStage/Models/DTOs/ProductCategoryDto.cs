namespace Team1.VitalBridge.BackStage.Models.DTOs
{
    public class ProductCategoryDto
    {
        // 這是傳送給前端的資料類別物件
        public int Id { get; set; }
        public int? FatherId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

		// Level 用來表示類別的層級，0 代表第一層，1 代表第二層，以此類推
		public int Level { get; set; }

		// FatherName 用來顯示父類別的名稱
		public string? FatherName { get; set; }
        
        // 儲存FildeTable圖片名稱
        public string? ImageFileName { get; set; } // 這邊要去接fileName名字


    }
    

}
