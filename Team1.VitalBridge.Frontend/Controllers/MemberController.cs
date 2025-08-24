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
    [Authorize]
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
            var userIdClaim = User.FindFirst("sub");
            if (userIdClaim == null) return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out var userId))
                return BadRequest("Invalid user id.");

            var profile = await _memberService.GetProfileAsync(userId);
            if (profile == null) return NotFound();

            return Ok(profile);
        }



    }
}
