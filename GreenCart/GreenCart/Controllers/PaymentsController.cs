using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GreenCart.Configuration;
using GreenCart.Dtos.Requests.Payments;
using GreenCart.Dtos.Responses.Payments;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IVnPayService _vnPayService;
        private readonly AppSettings _appSettings;

        public PaymentsController(IVnPayService vnPayService, IOptions<AppSettings> appSettings)
        {
            _vnPayService = vnPayService;
            _appSettings = appSettings.Value;
        }

       
        [HttpPost("vnpay/create")]
        [Authorize]
        public async Task<ActionResult<VnPayPaymentResponse>> CreateVnPayPayment([FromBody] CreateVnPayPaymentRequest request)
        {
            var userId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            try
            {
                var result = await _vnPayService.CreatePaymentUrlAsync(request.OrderId, userId, ipAddress);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var result = await _vnPayService.HandleReturnAsync(Request.Query);
                var frontendBaseUrl = _appSettings.FrontendBaseUrl.TrimEnd('/');

                if (result.IsSuccess)
                {
                    return Redirect($"{frontendBaseUrl}/index.html#payment-success?orderId={result.OrderId}&transactionId={result.TransactionId}&amount={result.Amount}");
                }
                else
                {
                    var encodedMessage = Uri.EscapeDataString(result.Message);
                    return Redirect($"{frontendBaseUrl}/index.html#payment-failed?orderId={result.OrderId}&message={encodedMessage}");
                }
            }
            catch (InvalidOperationException ex)
            {
                var frontendBaseUrl = _appSettings.FrontendBaseUrl.TrimEnd('/');
                var encodedMessage = Uri.EscapeDataString(ex.Message);
                return Redirect($"{frontendBaseUrl}/index.html#payment-failed?message={encodedMessage}");
            }
        }

        [HttpGet("vnpay-ipn")]
        [HttpPost("vnpay-ipn")]
        public async Task<ActionResult<VnPayIpnResponse>> VnPayIpn()
        {
            var response = await _vnPayService.HandleIpnAsync(Request.Query);
            return Ok(response);
        }

        [HttpGet("vnpay/status/{orderId:int}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            try
            {
                var status = await _vnPayService.GetTransactionStatusAsync(orderId);
                return Ok(new { orderId = orderId, paymentStatus = status.ToString() });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
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

        private string GetClientIpAddress()
        {
            var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ip) || ip == "::1")
            {
                ip = "127.0.0.1";
            }
            return ip;
        }
    }
}
