using FluentValidation;
using GreenCart.Dtos.Requests.Products;

namespace GreenCart.Validators.Products
{
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .MaximumLength(200)
                .When(x => x.Name != null);

            RuleFor(x => x.BasePrice)
                .GreaterThan(0)
                .When(x => x.BasePrice.HasValue)
                .WithMessage("Base price must be greater than zero.");

            RuleFor(x => x.SalePrice)
                .LessThanOrEqualTo(x => x.BasePrice!.Value)
                .When(x => x.SalePrice.HasValue && x.BasePrice.HasValue)
                .WithMessage("Sale price must be less than or equal to base price.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.StockQuantity.HasValue);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0)
                .When(x => x.CategoryId.HasValue);
        }
    }
}
