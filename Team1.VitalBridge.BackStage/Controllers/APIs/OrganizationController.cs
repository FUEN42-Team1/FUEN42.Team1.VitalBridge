using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Team1.VitalBridge.BackStage.Models.EFModels;
using static OrderController;

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    public class OrganizationRequest
    {
        public string City { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly AppDbContext context;

        public OrganizationController(AppDbContext _context)
        {
            this.context = _context;
        }

        [AllowAnonymous]
        [HttpPost("getOrganization")]
        public IActionResult GetOrganization([FromBody] OrganizationRequest request)
        {
            var Organizations = context.Organizations
                .Where(o => o.Address.Contains(request.City)).Select(o => new OrganizationDTO
                {
                    Name = o.Name,
                    Address = o.Address,
                    BedCount = o.BedCount,
                    Type = o.Type.Name

                }).ToList();
            if (request.Type != "全部")
                Organizations = Organizations.Where(o => o.Type.Contains(request.Type)).ToList();

            if (Organizations.Count==0) return NotFound(new { message = "查無符合機構" });
            return Ok(Organizations);
        }
        public class OrganizationDTO
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public string Address { get; set; }
            public int BedCount { get; set; }
        }
    }
}
