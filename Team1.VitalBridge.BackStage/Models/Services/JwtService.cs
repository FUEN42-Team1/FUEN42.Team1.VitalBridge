using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Team1.VitalBridge.BackStage.Models.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(List<Claim> claims, double expirationMinutes, string audienceKey)
        {
            var secretKey = _configuration["JwtSettings:Secret"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration[$"JwtSettings:{audienceKey}"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT Settings (Secret, Issuer, Audience) are not properly configured.");
            }

            var key = Encoding.UTF8.GetBytes(secretKey);
            var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public void SetTokenCookie(string token, double expirationMinutes, HttpResponse response, string cookieName, string path)
        {
            var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

            response.Cookies.Append(cookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // 生產環境務必設定為 true (HTTPS)
                SameSite = SameSiteMode.Lax, // 建議用於防範 CSRF
                Expires = expires,
                Path = path // 設定 Cookie 的有效路徑
            });
        }

        // 清除指定名稱和路徑的 Cookie
        public void ClearTokenCookie(HttpResponse response, string cookieName, string path)
        {
            response.Cookies.Delete(cookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = path
            });
        }



    }
}
