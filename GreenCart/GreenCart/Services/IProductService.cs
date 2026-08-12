using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Products;
using GreenCart.Dtos.Responses.Products;
using GreenCart.Repositories.Helpers;

namespace GreenCart.Services
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(CreateProductRequest request);
        Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductResponse?> GetProductByIdAsync(int id);
        Task<PagedResult<ProductResponse>> GetProductsAsync(ProductFilterParams filterParams);
    }
}
