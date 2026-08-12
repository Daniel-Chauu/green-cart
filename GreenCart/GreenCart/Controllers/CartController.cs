using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Cart;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.GetCartAsync(userId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _cartService.AddToCartAsync(userId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{productId:int}")]
        public async Task<IActionResult> UpdateCartItem(int productId, [FromQuery] int quantity)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _cartService.UpdateCartItemAsync(userId, productId, quantity);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, productId);
            if (!result)
                return NotFound(new { message = "Cart item not found." });

            return Ok(new { message = "Item removed from cart." });
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(new { message = "Cart cleared." });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User identity not found.");
            return userId;
        }
    }
}
