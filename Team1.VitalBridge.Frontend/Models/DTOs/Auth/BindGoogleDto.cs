namespace Team1.VitalBridge.Frontend.Models.DTOs.Auth
{
    public class BindGoogleDto
    {
        public string ProviderKey { get; set; } // Google OAuth 回傳的 sub
        public string Email { get; set; }
    }
}
