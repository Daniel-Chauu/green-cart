using System;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Cart;
using GreenCart.Dtos.Responses.Cart;
using GreenCart.Entities;
using GreenCart.Repositories;

namespace GreenCart.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CartResponse> GetCartAsync(int userId)
        {
            var cartItems = await _unitOfWork.CartItems.FindAsync(c => c.UserId == userId);
            var response = new CartResponse();

            foreach (var item in cartItems)
            {
                var product = await _unitOfWork.Products.GetProductByIdWithDetailsAsync(item.ProductId);
                if (product == null) continue;

                var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary)
                    ?? product.Images.FirstOrDefault();

                response.Items.Add(new CartItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = product.Name,
                    ProductImageUrl = primaryImage?.ImageUrl,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    TotalPrice = item.UnitPrice * item.Quantity
                });
            }

            response.SubTotal = response.Items.Sum(i => i.TotalPrice);
            response.TotalItems = response.Items.Sum(i => i.Quantity);
            return response;
        }

        public async Task<CartItemResponse> AddToCartAsync(int userId, AddToCartRequest request)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId)
                ?? throw new KeyNotFoundException($"Product with ID {request.ProductId} not found.");

            if (!product.IsActive)
                throw new InvalidOperationException("Product is not available.");

            if (product.StockQuantity < request.Quantity)
                throw new InvalidOperationException($"Insufficient stock. Only {product.StockQuantity} items available.");

            // Check if item already exists in cart (unique constraint: UserId + ProductId)
            var existingItems = await _unitOfWork.CartItems.FindAsync(
                c => c.UserId == userId && c.ProductId == request.ProductId);
            var existingItem = existingItems.FirstOrDefault();

            CartItem cartItem;
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UnitPrice = product.SalePrice ?? product.BasePrice;
                _unitOfWork.CartItems.Update(existingItem);
                cartItem = existingItem;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.SalePrice ?? product.BasePrice
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();

            return new CartItemResponse
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                TotalPrice = cartItem.UnitPrice * cartItem.Quantity
            };
        }

        public async Task<CartItemResponse> UpdateCartItemAsync(int userId, int productId, int quantity)
        {
            var existingItems = await _unitOfWork.CartItems.FindAsync(
                c => c.UserId == userId && c.ProductId == productId);
            var cartItem = existingItems.FirstOrDefault()
                ?? throw new KeyNotFoundException("Cart item not found.");

            if (quantity <= 0)
            {
                _unitOfWork.CartItems.Delete(cartItem);
                await _unitOfWork.SaveChangesAsync();
                return new CartItemResponse { ProductId = productId, Quantity = 0 };
            }

            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw new KeyNotFoundException("Product not found.");

            if (product.StockQuantity < quantity)
                throw new InvalidOperationException($"Insufficient stock. Only {product.StockQuantity} available.");

            cartItem.Quantity = quantity;
            cartItem.UnitPrice = product.SalePrice ?? product.BasePrice;
            _unitOfWork.CartItems.Update(cartItem);
            await _unitOfWork.SaveChangesAsync();

            return new CartItemResponse
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                TotalPrice = cartItem.UnitPrice * cartItem.Quantity
            };
        }

        public async Task<bool> RemoveFromCartAsync(int userId, int productId)
        {
            var items = await _unitOfWork.CartItems.FindAsync(
                c => c.UserId == userId && c.ProductId == productId);
            var cartItem = items.FirstOrDefault();
            if (cartItem == null) return false;

            _unitOfWork.CartItems.Delete(cartItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task ClearCartAsync(int userId)
        {
            var items = await _unitOfWork.CartItems.FindAsync(c => c.UserId == userId);
            foreach (var item in items)
            {
                _unitOfWork.CartItems.Delete(item);
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
