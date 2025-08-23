using Team1.VitalBridge.Frontend.Models.DTOs.Member;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Interfaces
{
    public interface IMemberService
    {
        Task<User?> GetProfileAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task<bool> ChangePasswordAsync(int userId, string oldPwd, string newPwd);
        Task<bool> BindGoogleAsync(int userId, string googleToken);
        Task UnbindGoogleAsync(int userId);
    }
}
