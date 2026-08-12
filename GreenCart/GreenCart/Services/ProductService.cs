using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Products;
using GreenCart.Dtos.Responses.Products;
using GreenCart.Entities;
using GreenCart.Repositories;
using GreenCart.Repositories.Helpers;
using GreenCart.Services.Common;

namespace GreenCart.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorage;

        public ProductService(IUnitOfWork unitOfWork, IFileStorageService fileStorage)
        {
            _unitOfWork = unitOfWork;
            _fileStorage = fileStorage;
        }

        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            if (!await _unitOfWork.Products.IsSkuUniqueAsync(request.SKU))
                throw new InvalidOperationException($"SKU '{request.SKU}' already exists.");

            var product = new Product
            {
                Name = request.Name.Trim(),
                Slug = GenerateSlug(request.Name),
                SKU = request.SKU.Trim().ToUpper(),
                ShortDescription = request.ShortDescription?.Trim(),
                Description = request.Description.Trim(),
                BasePrice = request.BasePrice,
                SalePrice = request.SalePrice,
                StockQuantity = request.StockQuantity,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                IsActive = request.IsActive
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var inventory = new Inventory
            {
                ProductId = product.Id,
                Quantity = request.StockQuantity,
                LastRestockedAt = DateTime.UtcNow
            };
            await _unitOfWork.Inventories.AddAsync(inventory);

            if (request.Images != null && request.Images.Count > 0)
            {
                for (int i = 0; i < request.Images.Count; i++)
                {
                    var imageUrl = await _fileStorage.SaveProductImageAsync(request.Images[i], product.Id);
                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = i == 0,
                        DisplayOrder = i,
                        AltText = product.Name
                    };
                    await _unitOfWork.ProductImages.AddAsync(productImage);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(product.Id);
            return MapToResponse(created!);
        }

        public async Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request)
        {
            var product = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(id)
                ?? throw new KeyNotFoundException($"Product with ID {id} not found.");

            if (request.Name != null)
            {
                product.Name = request.Name.Trim();
                product.Slug = GenerateSlug(request.Name);
            }
            if (request.ShortDescription != null) product.ShortDescription = request.ShortDescription.Trim();
            if (request.Description != null) product.Description = request.Description.Trim();
            if (request.BasePrice.HasValue) product.BasePrice = request.BasePrice.Value;
            if (request.SalePrice.HasValue) product.SalePrice = request.SalePrice.Value;
            if (request.StockQuantity.HasValue)
            {
                product.StockQuantity = request.StockQuantity.Value;
                if (product.Inventory != null)
                {
                    product.Inventory.Quantity = request.StockQuantity.Value;
                }
            }
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;
            if (request.BrandId.HasValue) product.BrandId = request.BrandId.Value;
            if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;

            if (request.RemoveImageIds != null && request.RemoveImageIds.Count > 0)
            {
                var imagesToRemove = product.Images.Where(i => request.RemoveImageIds.Contains(i.Id)).ToList();
                foreach (var img in imagesToRemove)
                {
                    await _fileStorage.DeleteFileAsync(img.ImageUrl);
                    _unitOfWork.ProductImages.Delete(img);
                }
            }

            if (request.NewImages != null && request.NewImages.Count > 0)
            {
                int currentMax = product.Images.Any() ? product.Images.Max(i => i.DisplayOrder) : -1;
                for (int i = 0; i < request.NewImages.Count; i++)
                {
                    var imageUrl = await _fileStorage.SaveProductImageAsync(request.NewImages[i], product.Id);
                    var productImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = !product.Images.Any(x => x.IsPrimary) && i == 0,
                        DisplayOrder = currentMax + 1 + i,
                        AltText = product.Name
                    };
                    await _unitOfWork.ProductImages.AddAsync(productImage);
                }
            }

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(id);
            return MapToResponse(updated!);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(id);
            if (product == null) return false;

            foreach (var img in product.Images)
            {
                await _fileStorage.DeleteFileAsync(img.ImageUrl);
            }

            return await _unitOfWork.Products.SoftDeleteAsync(id);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(id);
            return product == null ? null : MapToResponse(product);
        }

        public async Task<PagedResult<ProductResponse>> GetProductsAsync(ProductFilterParams filterParams)
        {
            var result = await _unitOfWork.Products.GetFilteredProductsAsync(filterParams);
            return new PagedResult<ProductResponse>(
                result.Items.Select(MapToResponse).ToList(),
                result.TotalItems,
                result.PageNumber,
                result.PageSize
            );
        }

        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                SKU = product.SKU,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                BasePrice = product.BasePrice,
                SalePrice = product.SalePrice,
                StockQuantity = product.StockQuantity,
                RatingAverage = product.RatingAverage,
                ReviewCount = product.ReviewCount,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name,
                CreatedAt = product.CreatedAt,
                Images = product.Images.Select(i => new ProductImageResponse
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder,
                    AltText = i.AltText
                }).ToList()
            };
        }

        private static string GenerateSlug(string name)
        {
            var slug = name.ToLower().Trim();
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');
            return $"{slug}-{DateTime.UtcNow.Ticks % 100000}";
        }
    }
}
