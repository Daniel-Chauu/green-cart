using System.Threading.Tasks;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

    
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _dashboardService.GetSummaryAsync();
            return Ok(result);
        }

       
        [HttpGet("top-selling")]
        public async Task<IActionResult> GetTopSelling([FromQuery] int count = 5)
        {
            var result = await _dashboardService.GetTopSellingProductsAsync(count);
            return Ok(result);
        }
    }
}
