using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.BackStage.Models.ViewModels
{
    public class OrganizationRoomViewModel
    {
        public int Id { get; set; }
        public int? OrganizationId { get; set; }

        [Display(Name = "房型")]
        [Required(ErrorMessage = "{0} 為必選")]
        public int? RoomTypeId { get; set; }

        [Display(Name = "房型名稱")]
        public string? RoomTypeName { get; set; }

        [Display(Name = "月租價格")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "{0} 為必填")] // 如果這個房型區塊存在，這個欄位就必填

        public int? MonthlyPrice { get; set; }

        [Display(Name = "房型數量")]
        [Required(ErrorMessage = "{0} 為必填")] // 如果這個房型區塊存在，這個欄位就必填
        [Range(1, int.MaxValue, ErrorMessage = "{0} 必須大於 0")]
        public int? RoomQuantity { get; set; }

        [Display(Name = "是否需押金")]
        public bool? HasDeposit { get; set; }

        [Display(Name = "押金金額")]
        [DataType(DataType.Currency)]
        [CustomValidation(typeof(OrganizationRoomViewModel), "ValidateDepositAmount")]

        public int? DepositAmount { get; set; }

        [Display(Name = "押金月數")]
        [CustomValidation(typeof(OrganizationRoomViewModel), "ValidateDepositMonths")]
        public decimal? DepositMonths { get; set; }


        // 修正後的驗證方法，參數型態與 DepositAmount 屬性一致
        public static ValidationResult ValidateDepositAmount(int? depositAmount, ValidationContext validationContext)
        {
            var viewModel = (OrganizationRoomViewModel)validationContext.ObjectInstance;
            if (viewModel.HasDeposit == true && !depositAmount.HasValue)
            {
                return new ValidationResult("當需要押金時，押金金額為必填", new[] { validationContext.MemberName });
            }
            if (depositAmount.HasValue && depositAmount.Value < 0)
            {
                return new ValidationResult("押金金額必須為非負數", new[] { validationContext.MemberName });
            }
            return ValidationResult.Success;
        }

        public static ValidationResult ValidateDepositMonths(decimal? depositMonths, ValidationContext validationContext)
        {
            var viewModel = (OrganizationRoomViewModel)validationContext.ObjectInstance;
            if (viewModel.HasDeposit == true && !depositMonths.HasValue)
            {
                return new ValidationResult("當需要押金時，押金月數為必填", new[] { validationContext.MemberName });
            }
            if (depositMonths.HasValue && depositMonths.Value < 0)
            {
                return new ValidationResult("押金月數必須為非負數", new[] { validationContext.MemberName });
            }
            return ValidationResult.Success;
        }
    }
}
