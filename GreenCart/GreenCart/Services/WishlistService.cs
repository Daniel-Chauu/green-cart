using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Responses.Wishlist;
using GreenCart.Entities;
using GreenCart.Repositories;

namespace GreenCart.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WishlistService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<WishlistItemResponse>> GetUserWishlistAsync(int userId)
        {
            var items = await _unitOfWork.Wishlists.FindAsync(w => w.UserId == userId);
            var result = new List<WishlistItemResponse>();

            foreach (var item in items)
            {
                var product = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(item.ProductId);
                if (product == null) continue;

                var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary)
                    ?? product.Images.FirstOrDefault();

                result.Add(new WishlistItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    ProductImageUrl = primaryImage?.ImageUrl,
                    Price = product.SalePrice ?? product.BasePrice,
                    AddedAt = item.AddedAt
                });
            }

            return result;
        }

        public async Task<WishlistItemResponse> AddToWishlistAsync(int userId, int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw new KeyNotFoundException($"Product with ID {productId} not found.");

            var existing = await _unitOfWork.Wishlists.FindAsync(
                w => w.UserId == userId && w.ProductId == productId);

            if (existing.Any())
                throw new InvalidOperationException("Product is already in your wishlist.");

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductId = productId,
                AddedAt = DateTime.UtcNow
            };

            await _unitOfWork.Wishlists.AddAsync(wishlistItem);
            await _unitOfWork.SaveChangesAsync();

            return new WishlistItemResponse
            {
                Id = wishlistItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.SalePrice ?? product.BasePrice,
                AddedAt = wishlistItem.AddedAt
            };
        }

        public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
        {
            var items = await _unitOfWork.Wishlists.FindAsync(
                w => w.UserId == userId && w.ProductId == productId);
            var item = items.FirstOrDefault();
            if (item == null) return false;

            _unitOfWork.Wishlists.Delete(item);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
