using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.DTOs.ECShop;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentArticleProductBannerAPIController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContentArticleProductBannerAPIController(AppDbContext context)
        {
            this._context = context;
        }

        [HttpGet("{bannerId}")]
        public async Task<ActionResult<object>> GetProductByBannerId(int bannerId)
        {
            var product = _context.Products
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.CreateAt)
                    .Skip(bannerId - 1)
                    .FirstOrDefault();

            var banner = product == null ? null : new ContentArticleBannerDisplayDTO
            {
                Title = product.Name,
                subtitle = product.Keypoint,
                purchaseLink = $"https://localhost:7184/VitalBridge/ECshop/product-detail.html?id={product.Id}",
                ImageUrl = 
                    (product.ProductImages
                        .Where(pi => pi.File != null)
                        .OrderBy(pi => pi.SortOrder)
                        .Select(pi => pi.File.FileName)
                        .FirstOrDefault() ?? "")
                        //"https://localhost:7104/api/UploadFile/GetFile?fileName=" +
            };



            return Ok(banner);

        }

    }
}
