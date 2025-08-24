namespace Team1.VitalBridge.Frontend.Models.DTOs.Member
{
    public class MemberProfileDto
    {
        public string UserId { get; set; } = string.Empty;      // 會員編號
        public string Name { get; set; } = string.Empty;        // 姓名
        public string Phone { get; set; } = string.Empty;       // 電話
        public string Email { get; set; } = string.Empty;       // Email
        public int? CityId { get; set; }
        public string City { get; set; } = string.Empty;        // 縣市
        public int? TownshipId { get; set; }
        public string Township { get; set; } = string.Empty;    // 鄉鎮
        public string Address { get; set; } = string.Empty;     // 詳細地址
        public DateTime CreatedAt { get; set; }                 // 建立日期
        public DateTime UpdatedAt { get; set; }                 // 修改日期
        public DateTime? LastLoginAt { get; set; }
        public List<string> RoleNames { get; set; } = new();  // 角色名稱

    }
}
