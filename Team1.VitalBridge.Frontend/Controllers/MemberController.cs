using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.Frontend.Interfaces;
using Team1.VitalBridge.Frontend.Models.DTOs.Member;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<MemberProfileDto>> GetProfile()
        {
            // 取得目前登入者的 userId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) return Unauthorized();

            //var userIdClaim = User.FindFirst("sub");
            //if (userIdClaim == null) return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return BadRequest("Invalid user id.");

            var profile = await _memberService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            return Ok(profile);
        }

        [HttpGet("diag-auth")]
        [Authorize]   // 保持和 profile 一樣
        public ActionResult<object> DiagAuth()
        {
            return Ok(new
            {
                Authenticated = User.Identity?.IsAuthenticated,
                Name = User.Identity?.Name,
                Claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null) return Unauthorized("Missing user id claim.");
            if (!int.TryParse(userIdClaim.Value, out var userId))
                return BadRequest("Invalid user id.");

            var result = await _memberService.UpdateProfileAsync(userId, dto);
            if (!result) return BadRequest();
            return NoContent();
        }



    }
}
