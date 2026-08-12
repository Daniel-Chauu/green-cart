using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Data;
using GreenCart.Entities;
using GreenCart.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GreenCart.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Product?> GetProductByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Inventory)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .Include(p => p.Reviews.Where(r => r.IsApproved))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product?> GetProductBySlugAsync(string slug)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Inventory)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Slug == slug.ToLower());
        }

        public async Task<PagedResult<Product>> GetFilteredProductsAsync(ProductFilterParams filterParams)
        {
            IQueryable<Product> query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Inventory)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder));

            // Search filter
            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                var term = filterParams.SearchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.SKU.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term) ||
                    (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)));
            }

            // Category filter
            if (filterParams.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filterParams.CategoryId.Value);
            }

            // Brand filter
            if (filterParams.BrandId.HasValue)
            {
                query = query.Where(p => p.BrandId == filterParams.BrandId.Value);
            }

            // IsActive filter
            if (filterParams.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filterParams.IsActive.Value);
            }

            // Price Range filter
            if (filterParams.MinPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.BasePrice) >= filterParams.MinPrice.Value);
            }

            if (filterParams.MaxPrice.HasValue)
            {
                query = query.Where(p => (p.SalePrice ?? p.BasePrice) <= filterParams.MaxPrice.Value);
            }

            // Sorting
            query = filterParams.SortBy?.ToLower() switch
            {
                "price_asc" => query.OrderBy(p => p.SalePrice ?? p.BasePrice),
                "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.BasePrice),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "rating" => query.OrderByDescending(p => p.RatingAverage),
                _ => query.OrderByDescending(p => p.CreatedAt) // default newest
            };

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalItems, filterParams.PageNumber, filterParams.PageSize);
        }

        public async Task<IReadOnlyList<Product>> GetFeaturedProductsAsync(int count = 8)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.RatingAverage)
                .ThenByDescending(p => p.ReviewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Images.Where(i => i.IsPrimary))
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null)
        {
            if (excludeProductId.HasValue)
            {
                return !await _dbSet.AnyAsync(p => p.SKU.ToLower() == sku.ToLower() && p.Id != excludeProductId.Value);
            }
            return !await _dbSet.AnyAsync(p => p.SKU.ToLower() == sku.ToLower());
        }
    }
}
