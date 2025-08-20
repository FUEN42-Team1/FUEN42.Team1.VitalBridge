namespace Team1.VitalBridge.Frontend.Models.DTOs.Orgs
{
    /// <summary>
    /// 房型資訊DTO
    /// </summary>
    public class RoomInfoDto
    {
        /// <summary>
        /// 房型ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 房型名稱
        /// </summary>
        public string RoomTypeName { get; set; } = string.Empty;

        /// <summary>
        /// 月費
        /// </summary>
        public int? MonthlyPrice { get; set; }

        /// <summary>
        /// 房間數量
        /// </summary>
        public int? RoomQuantity { get; set; }

        /// <summary>
        /// 是否有保證金
        /// </summary>
        public bool? HasDeposit { get; set; }

        /// <summary>
        /// 保證金金額
        /// </summary>
        public int? DepositAmount { get; set; }

        /// <summary>
        /// 保證金月數
        /// </summary>
        public decimal? DepositMonths { get; set; }
    }
}