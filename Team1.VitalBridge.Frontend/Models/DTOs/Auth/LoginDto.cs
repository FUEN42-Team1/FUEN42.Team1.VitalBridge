using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.Frontend.Models.DTOs.Auth
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
        public string? RecaptchaToken { get; set; } // 前端需要時才會帶
    }
}