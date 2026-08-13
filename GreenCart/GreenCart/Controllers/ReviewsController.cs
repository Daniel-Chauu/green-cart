using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Reviews;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpGet("products/{productId:int}")]
        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var result = await _reviewService.GetProductReviewsAsync(productId);
            return Ok(result);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAdminReviews([FromQuery] AdminReviewFilterRequest filter)
        {
            var result = await _reviewService.GetAdminReviewsAsync(filter);
            return Ok(result);
        }

        [HttpPut("admin/{id:int}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ApproveReview(int id)
        {
            try
            {
                await _reviewService.ApproveReviewAsync(id);
                return Ok(new { message = "Review approved successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Reject and remove a customer review. Admin & Staff only.
        /// </summary>
        [HttpPut("admin/{id:int}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> RejectReview(int id)
        {
            try
            {
                await _reviewService.RejectReviewAsync(id);
                return Ok(new { message = "Review rejected successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _reviewService.CreateReviewAsync(userId, request);
                return CreatedAtAction(nameof(GetProductReviews), new { productId = request.ProductId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            try
            {
                var result = await _reviewService.DeleteReviewAsync(id, userId, isAdmin);
                if (!result)
                    return NotFound(new { message = "Review not found." });

                return Ok(new { message = "Review deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
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
