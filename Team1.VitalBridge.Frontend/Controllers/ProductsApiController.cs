using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsApiController(AppDbContext context)
        {
            this._context=context;
        }


        //GET api/Products/Homepage
        /// <summary>
        /// 取得首頁商品資料，包含四個分類的推薦商品
        /// </summary>
        /// <returns>回傳首頁各分類商品清單</returns>

        [HttpGet("Homepage")]
        public async Task<IActionResult> GetHomepageProducts()
        {
            // 實作邏輯
            try
            {
                var response = new HomepageProductsResponseDto();

                // 1. 取得銀髮食品 (大分類 ID=5 的子分類商品)
                response.SilverFood = await GetProductsByParentCategoryId(5, 4);

                // 2. 取得保健食品 (大分類 ID=6 的子分類商品)  
                response.HealthFood = await GetProductsByParentCategoryId(6, 4);

                // 3. 取得健康飲品 (大分類 ID=15 的子分類商品)
                response.HealthDrink = await GetProductsByParentCategoryId(15, 4);

                // 4. 取得保健照護 (直接取分類 ID=10 的商品)
                response.HealthCare = await GetProductsByCategoryId(10, 4);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取得首頁商品資料時發生錯誤", error = ex.Message });
            }

        }

        // 直接透過分類 ID 取得商品 (用於保健照護)
        private async Task<List<ProductHomepageDto>> GetProductsByCategoryId(int categoryId, int takeCount)
        {
            return await _context.Products
                .Where(p => p.IsActive &&
                           p.ProductCategories.Any(pc => pc.CategoryId == categoryId))
                .OrderByDescending(p => p.CreateAt) // 按創建時間排序
                .Take(takeCount)
                .Select(p => new ProductHomepageDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Keypoint = p.Keypoint,
                    Price = p.Price,
                    ImageFileName = p.ProductImages
                        .Where(pi => pi.File != null)
                        .OrderBy(pi => pi.SortOrder)
                        .Select(pi => pi.File.FileName)
                        .FirstOrDefault() ?? "",
                    CategoryId = categoryId,
                    CategoryName = p.ProductCategories
                        .Where(pc => pc.CategoryId == categoryId)
                        .Select(pc => pc.Category.Name)
                        .FirstOrDefault() ?? ""
                })
                .ToListAsync();
        }

        // // 透過父分類 ID 取得商品 (用於有子分類的大分類)
        private async Task<List<ProductHomepageDto>> GetProductsByParentCategoryId(int parentCategoryId, int takeCount)
        {
            // 1. 先找出該父分類下的所有子分類 ID
            var childCategoryIds = await _context.Categories
                .Where(c => c.FatherId == parentCategoryId && c.IsActive)
                .Select(c => c.Id)
                .ToListAsync();

            if (!childCategoryIds.Any())
                return new List<ProductHomepageDto>();

            // 2. 查詢這些子分類下的商品
            return await _context.Products
                .Where(p => p.IsActive &&
                           p.ProductCategories.Any(pc => childCategoryIds.Contains(pc.CategoryId)))
                .OrderByDescending(p => p.CreateAt) // 按創建時間排序
                .Take(takeCount)
                .Select(p => new ProductHomepageDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Keypoint = p.Keypoint,
                    Price = p.Price,
                    ImageFileName = p.ProductImages
                        .Where(pi => pi.File != null)
                        .OrderBy(pi => pi.SortOrder)
                        .Select(pi => pi.File.FileName)
                        .FirstOrDefault() ?? "",
                    CategoryId = p.ProductCategories
                        .Where(pc => childCategoryIds.Contains(pc.CategoryId))
                        .Select(pc => pc.CategoryId)
                        .FirstOrDefault(),
                    CategoryName = p.ProductCategories
                        .Where(pc => childCategoryIds.Contains(pc.CategoryId))
                        .Select(pc => pc.Category.Name)
                        .FirstOrDefault() ?? ""
                })
                .ToListAsync();
        }
    }
}
