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
            var user = await _db.Users
                .Include(u => u.MemberProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            string cityName = "";
            string townshipName = "";
            string address = user.MemberProfile?.Address ?? "";

            if (user.MemberProfile?.CityId != null)
            {
                var city = await _db.Citys
                    .Where(c => c.Id == user.MemberProfile.CityId.Value)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
                cityName = city ?? "";
            }
            if (user.MemberProfile?.TownshipId != null)
            {
                var township = await _db.Townships
                    .Where(t => t.Id == user.MemberProfile.TownshipId.Value)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync();
                townshipName = township ?? "";
            }

            return new MemberProfileDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email,
                City = cityName,
                Township = townshipName,
                Address = address,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };
        }


        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;

            user.Name = dto.Name;
            user.Phone = dto.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPwd, string newPwd)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return false;


            if (!HashUtility.VerifyPassword(user.Password, oldPwd)) return false;

            user.Password = HashUtility.HashPassword(newPwd);
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BindGoogleAsync(int userId, string googleToken)
        {
            // 這裡應該要驗證 googleToken 並取得 Google 帳號資訊
            // 省略 Google 驗證細節，僅示範資料儲存
            var googleLogin = new ExternalLogin
            {
                UserId = userId,
                // Provider, ProviderKey 等欄位請依你的資料表設計補上
            };
            _db.ExternalLogins.Add(googleLogin);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task UnbindGoogleAsync(int userId)
        {
            var logins = _db.ExternalLogins.Where(x => x.UserId == userId /* && x.Provider == "Google" */);
            _db.ExternalLogins.RemoveRange(logins);
            await _db.SaveChangesAsync();
        }
    }
}
