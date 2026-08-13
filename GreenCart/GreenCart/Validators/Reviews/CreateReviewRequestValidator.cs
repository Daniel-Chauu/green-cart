using FluentValidation;
using GreenCart.Dtos.Requests.Reviews;

namespace GreenCart.Validators.Reviews
{
    public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
    {
        public CreateReviewRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID is required.");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5 stars.");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}
