using FluentValidation;
using GreenCart.Dtos.Requests.Products;

namespace GreenCart.Validators.Products
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200);

            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(100);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).WithMessage("Base price must be greater than zero.");

            RuleFor(x => x.SalePrice)
                .LessThanOrEqualTo(x => x.BasePrice)
                .When(x => x.SalePrice.HasValue)
                .WithMessage("Sale price must be less than or equal to base price.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required.");
        }
    }
}
