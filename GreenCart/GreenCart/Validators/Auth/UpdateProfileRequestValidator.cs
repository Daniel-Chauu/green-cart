using FluentValidation;
using GreenCart.Dtos.Requests.Auth;

namespace GreenCart.Validators.Auth
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.")
                .When(x => x.FullName != null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.")
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.")
                .When(x => x.Address != null);
        }
    }
}
