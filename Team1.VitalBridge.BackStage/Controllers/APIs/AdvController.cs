using Team1.VitalBridge.BackStage.Models.EFModels;
using Team1.VitalBridge.BackStage.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Team1.VitalBridge.BackStage.Controllers.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdvController(AppDbContext context)
        {
            this._context = context;
        }

        // GET: api/<AdvController>
        [HttpGet]
        public IActionResult Get(string route, DateOnly targetDate)
        {
            var plate = _context.Plates
                .Include(p=>p.DefaultImageFile)
                .Where(p=>(p.Route==route || "/VitalBridge" + p.Route== route) && p.Enable)
                .OrderBy(p => p.Id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Route,
                    p.Location,
                    p.DefaultImageIntroduct,
                    p.DefaultImageFileId,
                    p.DefaultImageClickUrl,
                    p.DefaultImageFile,
                    Images = p.PlateImages
                    .Where(img => img.StartDate <= targetDate && img.EndDate >= targetDate)
                    .Select(img => new
                    {
                        img.Id,
                        img.ImageFileId,
                        img.Introduct,
                        img.StartDate,
                        img.EndDate,
                        img.DisplayOrder,
                        img.ClickUrl,
                        img.ClickNumber,
                        img.ImageFile
                    })
                    .OrderBy(img=>img.DisplayOrder)
                    .ToList()
                })
                .ToList();
            return Ok(plate);
        }
        [HttpPost]
        public IActionResult Post([FromForm] int id, [FromForm] string name, [FromForm] string? defualImageIntroduct, [FromForm] string defualImageUrl, [FromForm] string? defualImageClickUrl)
        {
            var plate = _context.Plates.Find(id);
            var defualImageFileId = _context.FileStreams.FirstOrDefault(f => f.FileName == defualImageUrl).Id;
            if (plate == null) return NotFound();
            plate.Name = name;
            plate.DefaultImageClickUrl = string.IsNullOrEmpty(defualImageClickUrl) ? null : defualImageClickUrl;
            plate.DefaultImageFileId = defualImageFileId;
            plate.DefaultImageIntroduct = string.IsNullOrEmpty(defualImageIntroduct) ? null : defualImageIntroduct;
            _context.Plates.Update(plate);
            _context.SaveChanges();

            return Ok();
        }
        [HttpPut]
        public IActionResult ToggleEnable([FromForm] int id, [FromForm] bool enable)
        {
            var plate = _context.Plates.Find(id);
            if (plate == null) return NotFound();

            plate.Enable = enable;
            _context.SaveChanges();

            return Ok();
        }
        [HttpDelete]
        public IActionResult Delete([FromForm] int id)
        {
            var plate = _context.Plates.Find(id);
            _context.Plates.Remove(plate);
            _context.SaveChanges();

            return Ok();
        }
    }
}
