using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims; // 加入這行
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs;


namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _auth.RegisterAsync(dto);
            return Ok();
        }

        //[HttpPost("login")]
        //public async Task<ActionResult<TokenRes>> Login(LoginDto dto)
        //    => await _auth.LoginAsync(dto);

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _auth.LoginAsync(dto);
                return Ok(new { success = true, data = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "伺服器錯誤" });
            }
        }




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
    }
}
