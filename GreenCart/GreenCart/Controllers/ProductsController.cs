using System.Security.Claims;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Products;
using GreenCart.Repositories.Helpers;
using GreenCart.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenCart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductFilterParams filterParams)
        {
            var result = await _productService.GetProductsAsync(filterParams);
            return Ok(result);
        }

   
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found." });

            return Ok(product);
        }

       
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request)
        {
            try
            {
                var result = await _productService.CreateProductAsync(request);
                return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

      
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, request);
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

       
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
                return NotFound(new { message = $"Product with ID {id} not found." });

            return Ok(new { message = "Product deleted successfully." });
        }
    }
}
