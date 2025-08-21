using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // 加入這行
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.EFModels;


namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly AppDbContext _context;

        //public AuthController(IAuthService auth) => _auth = auth;

        public AuthController(AppDbContext context, IAuthService auth)
        {
            this._auth = auth;
            this._context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _auth.RegisterAsync(dto);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenRes>> Login(LoginDto dto)
            => await _auth.LoginAsync(dto);

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenRes>> Refresh()
            => await _auth.RefreshAsync();

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _auth.LogoutAsync();
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<object>> Me()
        {
            // 取得目前登入者的 id
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // 這裡假設 IAuthService 有 GetUserInfoAsync 方法
            var userInfo = await _auth.GetUserInfoAsync(userId);
            if (userInfo == null)
                return NotFound();

            return Ok(userInfo);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _auth.SendPasswordResetEmailAsync(dto.Email);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("密碼不一致");

            var result = await _auth.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!result)
                return BadRequest("Token 無效或已過期");
            return Ok();
        }
        [Authorize]
        [HttpGet("getNotify")]
        public async Task<ActionResult<object>> getNotify()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();
            var data = _context.NotifyUsers.Where(u => u.UserId == userId && u.IsRead ==false)
                .Include(u => u.Notify)
                .ThenInclude(n => n.Categories)
                .Where(u => u.Notify.SendDate <= DateTime.Now && (u.Notify.ValidityDate == null || u.Notify.ValidityDate > DateTime.Now))
                .Take(3)
                .Select(u => new
                {
                    u.Notify.Title,
                    u.Notify.Text,
                    u.Notify.NotifysUrl,
                    SendDate = u.Notify.SendDate.ToString("yyyy年MM月dd日 tt hh:mm", new System.Globalization.CultureInfo("zh-TW")),
                    u.Notify.Categories.Name,
                    u.IsRead
                })
                .ToList();
            if (data.Count == 0) return NotFound();
            return Ok(data);
        }

        [Authorize]
        [HttpGet("getAllNotify")]
        public async Task<ActionResult<object>> getAllNotify()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();
            var data = _context.NotifyUsers.Where(u => u.UserId == userId)
                .Include(u => u.Notify).ThenInclude(n => n.Categories).Where(u => u.Notify.SendDate <= DateTime.Now && (u.Notify.ValidityDate == null || u.Notify.ValidityDate > DateTime.Now))
                .Select(u => new
                {
                    u.Notify.Title,
                    u.Notify.Text,
                    u.Notify.NotifysUrl,
                    SendDate = u.Notify.SendDate.ToString("yyyy年MM月dd日 tt hh:mm", new System.Globalization.CultureInfo("zh-TW")),
                    u.Notify.Categories.Name,
                    u.IsRead
                })
                .ToList();
            return Ok(data);
        }

        [Authorize]
        [HttpGet("AllRead")]
        public async Task<ActionResult<object>> AllRead()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();
            var items = _context.NotifyUsers
            .Where(u => u.UserId == userId && !u.IsRead)
            .ToList();

            foreach (var item in items)
            {
                item.IsRead = true;
            }

            _context.SaveChanges();


            return Ok();
        }
    }
}
