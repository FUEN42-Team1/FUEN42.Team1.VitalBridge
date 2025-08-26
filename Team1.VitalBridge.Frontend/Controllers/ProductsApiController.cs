using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Team1.VitalBridge.Frontend.Models.DTOs;
using Team1.VitalBridge.Frontend.Models.DTOs.ECShop;
using Team1.VitalBridge.Frontend.Models.EFModels;
using Team1.VitalBridge.Frontend.Models.Services;

namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
		private readonly CategoryService _categoryService;

		public ProductsApiController(AppDbContext context, CategoryService categoryService)
        {
            this._context=context;
			this._categoryService = categoryService; // 注入 CategoryService
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

        //GET api/ProductsApi/Products
        /// <summary>
        /// 取得商品列表，支援分類篩選、關鍵字搜尋和分頁
        /// </summary>
        /// <param name="categoryId">分類ID（子分類）</param>
        /// <param name="searchQuery">搜尋關鍵字</param>
        /// <param name="page">頁碼（預設1）</param>
        /// <param name="pageSize">每頁數量（預設12，支援12/24/36/48）</param>
        /// <returns>商品列表和分頁資訊</returns>
        /// 

        [HttpGet("Products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? categoryId = null,
            [FromQuery] string searchQuery = null,
             [FromQuery] string sortBy = "newest",// 新增排序參數
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12)
        {
            try
            {
                // === 第一步：參數驗證和預設值設定 ===
                if (page < 1) page = 1;  // 頁碼不能小於1
                if (!new[] { 12, 24, 36, 48 }.Contains(pageSize))
                    pageSize = 12;  // 限制每頁數量只能是這四個值

                // === 第二步：建立基礎查詢 ===
                // 從所有啟用的商品開始查詢
                var query = _context.Products
                    .Where(p => p.IsActive)  // 只取啟用的商品
                    .AsQueryable();  // 建立可查詢的物件

                // === 第三步：分類篩選 ===
                if (categoryId.HasValue)
                {
                    // 透過 ProductCategories 中介表來篩選特定分類的商品
                    // Any() 表示「存在任何一個ProductCategory的CategoryId等於指定值」
                    query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId.Value));
                }

                // === 第四步：關鍵字搜尋（AND邏輯） ===
                if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    // 將搜尋字串分割成多個關鍵字
                    // 例如："鈣片 維他命" → ["鈣片", "維他命"]
                    var keywords = searchQuery.Trim()
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(k => !string.IsNullOrWhiteSpace(k))
                        .ToList();

                    // 對每個關鍵字使用 AND 邏輯
                    // 意思是商品必須同時包含所有關鍵字才會被找到
                    foreach (var keyword in keywords)
                    {
                        var normalizedKeyword = keyword.Trim();
                        query = query.Where(p =>
                            p.Name.Contains(normalizedKeyword) ||           // 商品名稱包含關鍵字
                            p.Keypoint.Contains(normalizedKeyword) ||       // 關鍵賣點包含關鍵字
                            p.ItemNumber.Contains(normalizedKeyword)        // 商品貨號包含關鍵字
                        );
                    }
                }
                // === 第五步：排序邏輯 ===
                switch (sortBy?.ToLower())
                {
                    case "price_asc":
                        query = query.OrderBy(p => p.Price);
                        break;
                    case "price_desc":
                        query = query.OrderByDescending(p => p.Price);
                        break;
                    case "newest":
                    default:
                        query = query.OrderByDescending(p => p.CreateAt);
                        break;
                }

                // === 第六步：計算總筆數和分頁資訊 ===
                var totalItems = await query.CountAsync();  // 符合條件的商品總數
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);  // 總頁數

                // === 第七步：執行分頁查詢並轉換成DTO ===
                var products = await query
                    
                    .Skip((page - 1) * pageSize)         // 跳過前面的資料
                    .Take(pageSize)                      // 只取這一頁的資料
                    .Select(p => new ProductListItemDto  // 轉換成DTO格式
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Keypoint = p.Keypoint,
                        Price = p.Price,
                        ItemNumber = p.ItemNumber,

                        // 取得商品的第一張圖片檔名
                        ImageFileName = p.ProductImages
                            .Where(pi => pi.File != null)      // 只要有檔案的圖片
                            .OrderBy(pi => pi.SortOrder)       // 按排序順序
                            .Select(pi => pi.File.FileName)    // 取檔案名稱
                            .FirstOrDefault() ?? "",           // 如果沒有圖片就回傳空字串

                        // 如果有指定分類，就用指定的；沒有就取商品的第一個分類
                        CategoryId = categoryId ?? p.ProductCategories
                            .Select(pc => pc.CategoryId)
                            .FirstOrDefault(),

                        // 取得分類名稱
                        CategoryName = categoryId.HasValue
                            ? p.ProductCategories
                                .Where(pc => pc.CategoryId == categoryId.Value)
                                .Select(pc => pc.Category.Name)
                                .FirstOrDefault() ?? ""
                            : p.ProductCategories
                                .Select(pc => pc.Category.Name)
                                .FirstOrDefault() ?? ""
                    })
                    .ToListAsync();  // 執行查詢並轉換成List

                // === 第七步：取得分類名稱（用於回應資訊） ===
                string categoryName = null;
                if (categoryId.HasValue)
                {
                    categoryName = await _context.Categories
                        .Where(c => c.Id == categoryId.Value)
                        .Select(c => c.Name)
                        .FirstOrDefaultAsync();
                }

                // === 第八步：建立完整的回應物件 ===
                var response = new ProductListResponseDto
                {
                    Products = products,  // 商品列表

                    // 分頁資訊
                    Pagination = new PaginationDto
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        PageSize = pageSize,
                        HasNextPage = page < totalPages,    // 是否有下一頁
                        HasPreviousPage = page > 1          // 是否有上一頁
                    },

                    // 搜尋資訊
                    SearchInfo = new SearchInfoDto
                    {
                        CategoryId = categoryId,
                        CategoryName = categoryName,
                        SearchQuery = searchQuery,
                        TotalFound = totalItems
                    }
                };

                return Ok(response);  // 回傳成功結果

            }
            catch (Exception ex) {
                // 錯誤處理：回傳500錯誤和錯誤訊息
                return StatusCode(500, new
                {
                    message = "取得商品列表時發生錯誤",
                    error = ex.Message
                });
            }
        }


        //GET api/ProductsApi/Product/5
        [HttpGet("Product/{id}")]
        public async Task<ActionResult<ProductDetailResponseDto>> GetProductDetail(int id)
        {
            try
            {
                // 1. 取得商品基本資料
            
                var product = await _context.Products
                    .Where(p => p.Id == id && p.IsActive) 
                    .Select(p => new ProductDetailDto
                    {
                        Id  = p.Id,
                        Name = p.Name,
                        ItemNumber = p.ItemNumber,
                        Keypoint = p.Keypoint,
                        ProductDescription = p.ProductDescription,
                        Price = p.Price,
                        Quantity = p.Quantity,
                        CreateAt = p.CreateAt,

                        // 取得所有商品圖片，直接轉換成 ProductImageDto
                        Images = p.ProductImages
                            .Where(pi => pi.File != null)
                            .OrderBy(pi => pi.SortOrder)
                            .Select(pi => new ProductImageDto
                            {
                                Id = pi.Id,
                                FileName = pi.File.FileName,
                                SortOrder = (int)pi.SortOrder
                            }).ToList(),

						// 重點：修正類別資訊查詢
						Categories = p.ProductCategories
                            .OrderBy(pc => pc.CategoryId) // 按ID排序，取最小的作為主要類別
							.Select(pc => new CategoryDto
                            {
                                Id=pc.Category.Id,
                                Name =pc.Category.Name, // 只取分類名稱
								FatherId = pc.Category.FatherId, // 🔥 加入父類別ID
								FatherName = pc.Category.Father.Name // 🔥 加入父類別名稱
							}).ToList()
                    })
                    .FirstOrDefaultAsync(); // 只取一筆

				if (product == null)
                    return NotFound(new { message = "商品不存在或已下架" });

                // 2. 取得最新的 ProductNote（送貨付款方式、購物須知），直接轉換成 ProductNoteDto
                var productNote = await _context.ProductNotes
                    .OrderByDescending(pn => pn.Id) // 假設 Id 是自增的，最新的會有最大的 Id
					.Select(pn => new ProductNoteDto
                    {
                        DeliveryAndPayMethod = pn.DeliveryAndPayMethod,
                        ShoppNote = pn.ShoppNote
                    })
                    .FirstOrDefaultAsync(); // 只取最新的一筆

				// 3. 組合回傳資料
				var response = new ProductDetailResponseDto
                {
                    Product = product,
                    ProductNote = productNote
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "取得商品詳情時發生錯誤",
                    error = ex.Message
                });
            }
        }
    }
}
