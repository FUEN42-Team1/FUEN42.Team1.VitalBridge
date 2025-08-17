using System.ComponentModel.DataAnnotations;

namespace Team1.VitalBridge.Frontend.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;
    }
}