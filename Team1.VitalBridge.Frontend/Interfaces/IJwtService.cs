using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Interfaces
{
    public interface IJwtService
    {
        string CreateAccessToken(User user, string[] roles);
        string GenerateRefreshToken(int bytes = 32);
        int AccessMinutes { get; }
        int RefreshDays { get; }
    }
}
