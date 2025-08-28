using Team1.VitalBridge.Frontend.Models.DTOs.Auth;

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
        Task<TokenRes?> LoginWithGoogleIdTokenAsync(string idToken); // Google 登入（驗證 ID Token）
        Task<TokenRes> LoginWithGoogleAsync(string email, string name, string providerKey); // Google 登入（已驗證）
        Task<bool> BindGoogleAsync(int userId, string providerKey, string email);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);// 修改密碼



    }
}
