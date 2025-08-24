using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Team1.VitalBridge.Frontend.Models.DTOs.ECShop;
using Team1.VitalBridge.Frontend.Models.EFModels;

namespace Team1.VitalBridge.Frontend.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CartApiController : ControllerBase
	{
		private readonly AppDbContext _context;

		public CartApiController(AppDbContext context)
		{
			this._context = context;
		}

		// POST: api/CartApi/AddItem
		[HttpPost("AddItem")]
		public async Task<IActionResult> AddItem([FromBody] AddCartItemDtocs dto)
		{
			try
			{
				// 1. 取得當前使用者ID
				var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
				if (!int.TryParse(userIdStr, out var userId))
					return Unauthorized("請先登入");


				// 2. 檢查商品是否存在且有庫存
				var product = await _context.Products
					.FirstOrDefaultAsync(p => p.Id == dto.ProductId && p.IsActive);
				if (product == null)
					return BadRequest("商品不存在或已下架");

				if (product.Quantity < dto.Quantity)
					return BadRequest($"庫存不足，目前庫存：{product.Quantity}");

				// 3. 取得或建立使用者的購物車
				var cart = await _context.Carts
					.FirstOrDefaultAsync(c => c.CustomerId == userId);
				if (cart == null)
				{
					cart = new Cart
					{
						CustomerId = userId,
						CreateAt = DateTime.Now
					};
					_context.Carts.Add(cart);
					await _context.SaveChangesAsync();
				}

				// 4. 檢查是否已有相同商品在購物車中
				// existingItem 代表購物車中已存在的商品項目，型別是物件CartItem
				// 如果找到符合條件的項目，existingItem 會被賦值為該項目；如果沒有找到，existingItem 會是 null
				var existingItem = await _context.CartItems
					.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == dto.ProductId);
				if (existingItem != null)
				{
					// 檢查累加後是否超過庫存
					if (existingItem.Quantity + dto.Quantity > product.Quantity)
						return BadRequest($"數量超過庫存，目前庫存：{product.Quantity}，購物車已有：{existingItem.Quantity}");

					// 檢查是否超過20個上限
					if (existingItem.Quantity + dto.Quantity > 20)
						return BadRequest($"單一商品最多20個，購物車已有：{existingItem.Quantity}");

					// 累加數量
					existingItem.Quantity += dto.Quantity;
					existingItem.Subtotal = existingItem.Quantity * product.Price;
				}
				else
				{
					// 檢查是否超過20個上限
					if (dto.Quantity > 20)
						return BadRequest("單一商品最多20個");

					// 新增商品到購物車
					var cartItem = new CartItem
					{
						ProductId = dto.ProductId,
						CartId = cart.Id,
						Quantity = dto.Quantity,
						Subtotal = dto.Quantity * product.Price,
						AddAt = DateTime.Now
					};
					_context.CartItems.Add(cartItem);
				}
				await _context.SaveChangesAsync();
				return Ok("成功加入購物車！");
			}
			catch (Exception ex)
			{
				return StatusCode(500, "加入購物車失敗");
			}



		}

		// 實作 GetCart API 目標：取得使用者的完整購物車資訊

		// GET: api/CartApi/GetCart
		[HttpGet]
		public async Task<ActionResult<CartResponseDto>> GetCart()
		{
			try
			{
				// 1. 取得當前使用者ID
				var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
				if (!int.TryParse(userIdStr, out var userId))
					return Unauthorized("請先登入");

				// 2. 取得使用者的購物車
				var cart = await _context.Carts
					.Include(c => c.CartItems)
					.ThenInclude(ci => ci.Product)
					.ThenInclude(p => p.ProductImages.Where(pi => pi.File != null))
					.ThenInclude(pi => pi.File)
					.FirstOrDefaultAsync(c => c.CustomerId == userId);

				if (cart == null)
				{
					// 如果沒有購物車，回傳空的
					return Ok(new CartResponseDto());
				}

				// 3. 篩選有效商品並建立 DTO
				var validItems = new List<CartItemDto>();
				var removedItems = new List<CartItem>();

				foreach (var cartItem in cart.CartItems)
				{
					// 檢查商品是否還有效
					if (cartItem.Product == null || !cartItem.Product.IsActive)
					{
						// 商品已下架，標記移除
						removedItems.Add(cartItem);
						continue;
					}

					// 建立購物車項目 DTO
					validItems.Add(new CartItemDto
					{
						Id = cartItem.Id,
						ProductId = cartItem.ProductId,
						ProductName = cartItem.Product.Name,
						UnitPrice = cartItem.Product.Price,
						Quantity = cartItem.Quantity,
						Subtotal = cartItem.Quantity * cartItem.Product.Price,
						Stock = cartItem.Product.Quantity, // 目前庫存
						ImageFileName = cartItem.Product.ProductImages
								.Where(pi => pi.File != null && pi.SortOrder == 1)
								.Select(pi => pi.File.FileName)
								.FirstOrDefault() ?? ""

					});

				}
				// 4. 移除已下架的商品
				if (removedItems.Any())
				{
					_context.CartItems.RemoveRange(removedItems);
					await _context.SaveChangesAsync();
				}
				// 5. 計算總計
				var response = new CartResponseDto
				{
					Items = validItems,
					TotalQuantity = validItems.Sum(item => item.Quantity),
					TotalAmount = validItems.Sum(item => item.Subtotal)
				};
				return Ok(response);
			}
			catch (Exception ex)
			{
				return StatusCode(500, "取得購物車失敗");
			}



		}

		// 實作 UpdateQuantity API 目標：更新購物車中某商品的數量
		// PUT: api/CartApi/UpdateQuantity
		[HttpPut("UpdateQuantity")]
		public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemDto dto)
		{
			try
			{
				// 1. 取得當前使用者ID
				var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
				if (!int.TryParse(userIdStr, out var userId))
					return Unauthorized("請先登入");

				// 2. 檢查數量是否有效
				if (dto.Quantity <= 0)
					return BadRequest("數量必須大於 0");

				if (dto.Quantity > 20)
					return BadRequest("單一商品最多 20 個");

				// 3. 取得購物車項目（確保是該使用者的）
				var cartItem = await _context.CartItems
					.Include(ci => ci.Cart)
					.Include(ci => ci.Product)
					.FirstOrDefaultAsync(ci => ci.Id == dto.CartItemId && ci.Cart.CustomerId == userId);

				if (cartItem == null)
					return NotFound("購物車項目不存在");
				// 4. 檢查商品是否還有效
				if (cartItem.Product == null || !cartItem.Product.IsActive)
					return BadRequest("商品已下架");

				// 5. 檢查庫存是否足夠
				if (dto.Quantity > cartItem.Product.Quantity)
					return BadRequest($"庫存不足，目前庫存：{cartItem.Product.Quantity}");

				// 6. 更新數量和小計
				cartItem.Quantity = dto.Quantity;
				cartItem.Subtotal = dto.Quantity * cartItem.Product.Price;

				// 7. 儲存變更
				await _context.SaveChangesAsync();

				return Ok("數量更新成功");
			}
			catch (Exception ex)
			{
				return StatusCode(500, "更新數量失敗");
			}

		}

		// DELETE : api/CartApi/RemoveItem/{cartItemId}
		[HttpDelete("RemoveItem/{cartItemId}")]
		public async Task<IActionResult> RemoveItem(int cartItemId)
		{
			try
			{
				// 1. 取得當前使用者ID
				var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
				if (!int.TryParse(userIdStr, out var userId))
					return Unauthorized("請先登入");

				// 2. 取得購物車項目（確保是該使用者的）
				var cartItem = await _context.CartItems
					.Include(ci => ci.Cart)
					.FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.CustomerId == userId);

				if (cartItem == null)
					return NotFound("購物車項目不存在");

				// 3. 移除項目
				_context.CartItems.Remove(cartItem);
				await _context.SaveChangesAsync();

				return Ok("商品已移除");


			}
			catch (Exception ex)
			{
				return StatusCode(500, "移除商品失敗");
			}

		}

		// 實作 GetCount API 這個 API 專門給導覽列使用，回傳購物車總數量

		// GET: api/CartApi/Count
		[HttpGet("Count")]
		public async Task<ActionResult<int>> GetCount()
		{
			try 
			{
				// 1. 取得當前使用者ID
				var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
				if (!int.TryParse(userIdStr, out var userId))
					return Ok(0); // 未登入回傳 0

				// 2. 計算購物車總數量
				var totalCount = await _context.CartItems
					.Where(ci => ci.Cart.CustomerId == userId && ci.Product.IsActive)
					.SumAsync(ci => ci.Quantity);
				
				return Ok(totalCount);
			}
			catch (Exception ex) 
			
			{
				return Ok(0); // 發生錯誤回傳 0，避免導覽列顯示異常
			}

		}

	}
}
