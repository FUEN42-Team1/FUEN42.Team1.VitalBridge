using Team1.VitalBridge.Frontend.Models.DTOs;

namespace Team1.VitalBridge.Frontend.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDto dto);
        Task<TokenRes> LoginAsync(LoginDto dto);
        Task<TokenRes> RefreshAsync();
        Task LogoutAsync();
        Task<object?> GetUserInfoAsync(int userId);
        Task SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> VerifyEmailAsync(string email, string token);
        Task<bool> ResendVerificationEmailAsync(string email);

    }
}
