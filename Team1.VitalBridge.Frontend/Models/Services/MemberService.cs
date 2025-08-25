using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.BackStage.Models.Utilities;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs.Member;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Models.Services
{
    public class MemberService : IMemberService
    {
        private readonly AppDbContext _db;

        public MemberService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MemberProfileDto?> GetProfileAsync(int userId)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            var profile = await _db.MemberProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId);

            var cityName = profile != null
                ? (await _db.Citys.FindAsync(profile.CityId))?.Name ?? ""
                : "";

            var townshipName = profile != null
                ? (await _db.Townships.FindAsync(profile.TownshipId))?.Name ?? ""
                : "";

            // 查詢所有角色名稱
            var roleNames = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            return new MemberProfileDto
            {
                UserId = user.UserId.ToString(),
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email,
                CityId = profile?.CityId,
                City = cityName,
                TownshipId = profile?.TownshipId,
                Township = townshipName,
                Address = profile?.Address ?? "",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                RoleNames = roleNames
            };
        }
        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            // 更新 User 基本資料
            user.Name = dto.Name;
            user.Phone = dto.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            // 更新 MemberProfile
            var profile = await _db.MemberProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                // 若沒有 profile，則新增
                profile = new MemberProfile
                {
                    UserId = userId,
                    CityId = dto.CityId,
                    TownshipId = dto.TownshipId,
                    Address = dto.Address
                };
                _db.MemberProfiles.Add(profile);
            }
            else
            {
                profile.CityId = dto.CityId;
                profile.TownshipId = dto.TownshipId;
                profile.Address = dto.Address;
            }

            await _db.SaveChangesAsync();
            return true;
        }












    }
}
