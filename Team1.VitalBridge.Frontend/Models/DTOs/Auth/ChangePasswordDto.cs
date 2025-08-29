using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.Frontend.Models.DTOs.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        [MinLength(6)]
        public string OldPassword { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = "";

        [Required]
        [MinLength(6)]
        public string ConfirmPassword { get; set; } = "";
    }
}
