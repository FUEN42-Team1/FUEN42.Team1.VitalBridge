using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team1.VitalBridge.Frontend.Models.EFModels;
using static Team1.VitalBridge.Frontend.Models.DTOs.ECShop.CheckoutDtos;
using Microsoft.Extensions.Options;


namespace Team1.VitalBridge.Frontend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 需要登入才能結帳
    public class CheckoutController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CheckoutController(AppDbContext context)
        {
            this._context=context;
        }
        /// <summary>
        /// 取得結帳初始化資料
        /// 包含：會員資料、購物車內容、可用物流方式、付款方式、地址資料
        /// </summary>
        /// 
        // GET : api/Checkout/init
        [HttpGet("init")]
        public async Task<IActionResult> GetCheckoutInit()
        {
            try 
            {
                // 1. 取得當前使用者ID
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized("請重新登入");

                // 2. 取得會員基本資料
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Name,
                        u.Email,
                        u.Phone
                    })
                    .FirstOrDefaultAsync();
                if (user == null)
                    return NotFound("找不到會員資料");

                // 3. 取得購物車內容
                var cartItems = await _context.CartItems
                    .Where(ci => ci.Cart.CustomerId == userId)
                    .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
                    .ThenInclude(pi => pi.File)
                    .Where(ci => ci.Product.IsActive) // 只取啟用的商品
                    .Select(ci => new CheckoutCartItemDto
                    {
                        CartItemId = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        UnitPrice = ci.Product.Price,
                        Quantity = ci.Quantity,
                        Subtotal = ci.Quantity * ci.Product.Price,
                        Stock = ci.Product.Quantity,
                        ImageFileName = ci.Product.ProductImages
                            .Where(pi => pi.File != null && pi.SortOrder == 1)
                            .Select(pi => pi.File.FileName)
                            .FirstOrDefault() ?? "",
                        ItemNumber = ci.Product.ItemNumber
                    })
                    .ToListAsync();


                // 檢查購物車是否為空
                if (!cartItems.Any())
                    return BadRequest("購物車是空的，無法結帳");

                // 4. 檢查商品庫存
                var stockIssues = cartItems.Where(item => item.Quantity > item.Stock).ToList();
                if (stockIssues.Any())
                {
                    return BadRequest(new
                    {
                        message = "部分商品庫存不足",
                        issues = stockIssues.Select(item => new
                        {
                            productName = item.ProductName,
                            requestedQuantity = item.Quantity,
                            availableStock = item.Stock
                        })
                    });
                }

                // 5. 檢查商品支援的物流方式
                var productIds = cartItems.Select(ci => ci.ProductId).ToList();
                var supportedShippingMethods = await _context.ProductShips
                    .Where(ps => productIds.Contains(ps.ProductId) && ps.IsActive) // 只取啟用的商品物流關聯
                    .Include(ps => ps.Ship) // 包含物流方式資料
                    .Where(ps => ps.Ship.IsActive == true) // 只取啟用的物流方式
                    .GroupBy(ps => ps.ShipId) // 依物流方式分組
                    .Where(g => g.Count() == productIds.Count) // 所有商品都支援的物流方式
                    .Select(g => new ShippingMethodDto
                    {
                        ShipId = g.Key,
                        ShipMethodName = g.First().Ship.ShipMethodName,
                        ShipCost = g.First().Ship.ShipCost
                    })
                    .ToListAsync();

                // 6. 取得可用付款方式
                var paymentMethods = await _context.PaymentMethods
                   .Where(pm => pm.IsActive)
                   .Select(pm => new PaymentMethodDto
                   {
                       Id = pm.Id,
                       Name = pm.Name
                   })
                   .ToListAsync();


                // 7. 取得縣市資料
                var cities = await _context.Citys
                    .OrderBy(c => c.Id)
                    .Select(c => new CityDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToListAsync();

                // 8. 計算總金額
                var subtotal = cartItems.Sum(item => item.Subtotal);


                // 組合回傳資料
                var response = new CheckoutInitResponseDto
                {
                    // 會員資料
                    CustomerInfo = new CustomerInfoDto
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Phone = user.Phone
                    },
                    // 購物車商品
                    CartItems = cartItems,
                    // 可用物流方式
                    AvailableShippingMethods = supportedShippingMethods,
                    // 付款方式
                    PaymentMethods = paymentMethods,
                    // 縣市資料
                    Cities = cities,
                    // 金額資訊
                    Subtotal = subtotal
                };

                return Ok(response);


            }
            catch (Exception ex)
            {
                // 記錄錯誤
                Console.WriteLine($"結帳初始化錯誤: {ex.Message}");
                return StatusCode(500, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 根據縣市ID取得鄉鎮區資料
        /// </summary>
        /// 
        [HttpGet("townships/{cityId}")]
        public async Task<IActionResult> GetTownships(int cityId)
        {
            try 
            {
                var townships = await _context.Townships
                    .Where(t => t.CityId == cityId)
                    .OrderBy(t => t.Id)
                    .Select(t => new TownshipDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        PostalCode = t.PostalCode ?? ""
                    })
                    .ToListAsync();
                return Ok(townships);

            } 
            catch (Exception ex)
            {
                Console.WriteLine($"取得鄉鎮區資料錯誤: {ex.Message}");
                return StatusCode(500, "取得地址資料失敗");

            }
        
        }

    }
}
