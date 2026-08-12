using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Addresses;
using GreenCart.Dtos.Responses.Addresses;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AddressResponse>>> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            var addresses = await _addressService.GetAddressesByUserIdAsync(userId);
            return Ok(addresses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AddressResponse>> GetAddressById(int id)
        {
            var userId = GetCurrentUserId();
            try
            {
                var address = await _addressService.GetAddressByIdAsync(id, userId);
                return Ok(address);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddressResponse>> CreateAddress([FromBody] CreateAddressRequest request)
        {
            var userId = GetCurrentUserId();
            var result = await _addressService.CreateAddressAsync(userId, request);
            return CreatedAtAction(nameof(GetAddressById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AddressResponse>> UpdateAddress(int id, [FromBody] UpdateAddressRequest request)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _addressService.UpdateAddressAsync(id, userId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = GetCurrentUserId();
            try
            {
                await _addressService.DeleteAddressAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/default")]
        public async Task<ActionResult<AddressResponse>> SetDefaultAddress(int id)
        {
            var userId = GetCurrentUserId();
            try
            {
                var result = await _addressService.SetDefaultAddressAsync(id, userId);
                return Ok(result);
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
    }
}
