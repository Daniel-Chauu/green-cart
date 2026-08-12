using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Entities;
using GreenCart.Repositories.Helpers;

namespace GreenCart.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product?> GetProductByIdWithDetailsAsync(int id);
        Task<Product?> GetProductBySlugAsync(string slug);
        Task<PagedResult<Product>> GetFilteredProductsAsync(ProductFilterParams filterParams);
        Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int count = 8);
        Task<IReadOnlyList<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);
    }
}
