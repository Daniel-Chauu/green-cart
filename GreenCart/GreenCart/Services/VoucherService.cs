using System;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Coupons;
using GreenCart.Dtos.Responses.Coupons;
using GreenCart.Entities;
using GreenCart.Repositories;

namespace GreenCart.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VoucherService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<VoucherValidationResponse> ValidateVoucherAsync(int userId, ApplyVoucherRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.VoucherCode))
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = "Voucher code cannot be empty."
                };
            }

            var voucherCode = request.VoucherCode.Trim().ToLower();

            // Find voucher by code
            var vouchers = await _unitOfWork.Vouchers.FindAsync(v => v.Code.ToLower() == voucherCode && !v.IsDeleted);
            var voucher = vouchers.FirstOrDefault();

            if (voucher == null)
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = $"Voucher code '{request.VoucherCode}' is invalid or does not exist."
                };
            }

            if (!voucher.IsActive)
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = "This voucher is currently inactive."
                };
            }

            if (voucher.StartDate > DateTime.UtcNow)
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = "This voucher is not active yet."
                };
            }

            if (voucher.EndDate < DateTime.UtcNow)
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = "This voucher has expired."
                };
            }

            if (voucher.UsageLimit > 0 && voucher.TimesUsed >= voucher.UsageLimit)
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = "This voucher usage limit has been reached."
                };
            }

            // Calculate user's current cart subtotal
            var cartItems = (await _unitOfWork.CartItems.FindAsync(c => c.UserId == userId)).ToList();
            if (!cartItems.Any())
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = "Your cart is empty. Cannot apply voucher."
                };
            }

            decimal cartSubTotal = 0;
            foreach (var cartItem in cartItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                if (product != null && product.IsActive && !product.IsDeleted)
                {
                    var price = product.SalePrice ?? product.BasePrice;
                    cartSubTotal += price * cartItem.Quantity;
                }
            }

            if (voucher.MinimumOrderAmount > 0 && cartSubTotal < voucher.MinimumOrderAmount)
            {
                return new VoucherValidationResponse
                {
                    IsValid = false,
                    Message = $"Cart subtotal ({cartSubTotal:C}) does not meet the minimum required amount ({voucher.MinimumOrderAmount:C}) for this voucher."
                };
            }

            // Calculate discount amount
            decimal discountAmount = 0;
            if (string.Equals(voucher.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
            {
                discountAmount = cartSubTotal * (voucher.DiscountValue / 100m);
                if (voucher.MaximumDiscountAmount.HasValue)
                {
                    discountAmount = Math.Min(discountAmount, voucher.MaximumDiscountAmount.Value);
                }
            }
            else
            {
                discountAmount = voucher.DiscountValue;
            }

            // Discount cannot exceed total cart subtotal
            discountAmount = Math.Min(discountAmount, cartSubTotal);

            return new VoucherValidationResponse
            {
                IsValid = true,
                Message = "Voucher applied successfully.",
                DiscountAmount = Math.Round(discountAmount, 2),
                DiscountType = voucher.DiscountType,
                VoucherCode = voucher.Code
            };
        }
    }
}
