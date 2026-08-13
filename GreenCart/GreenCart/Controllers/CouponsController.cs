using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Coupons;
using GreenCart.Dtos.Responses.Coupons;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CouponsController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public CouponsController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        /// <summary>
        /// Validate and calculate discount for a voucher against the user's current cart.
        /// (Does NOT place an order or redeem the voucher yet).
        /// </summary>
        [HttpPost("apply")]
        public async Task<ActionResult<VoucherValidationResponse>> ApplyVoucher([FromBody] ApplyVoucherRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _voucherService.ValidateVoucherAsync(userId, request);

            if (!result.IsValid)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User identity not found.");
            }
            return userId;
        }
    }
}
