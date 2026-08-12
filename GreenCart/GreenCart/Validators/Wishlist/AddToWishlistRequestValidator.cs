using FluentValidation;
using GreenCart.Dtos.Requests.Wishlist;

namespace GreenCart.Validators.Wishlist
{
    public class AddToWishlistRequestValidator : AbstractValidator<AddToWishlistRequest>
    {
        public AddToWishlistRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID is required.");
        }
    }
}
