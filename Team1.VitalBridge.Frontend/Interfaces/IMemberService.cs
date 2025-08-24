using Team1.VitalBridge.Frontend.Models.DTOs.Member;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Interfaces
{
    public interface IMemberService
    {
        Task<MemberProfileDto?> GetProfileAsync(int userId);
    }
}
