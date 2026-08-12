using FluentValidation;
using GreenCart.Dtos.Requests.Auth;
using GreenCart.Entities.Enums;

namespace GreenCart.Validators.Auth
{
    public class RegisterStaffRequestValidator : AbstractValidator<RegisterStaffRequest>
    {
        public RegisterStaffRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(150);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.Role)
                .Must(role => role == UserRole.Staff || role == UserRole.Admin)
                .WithMessage("Role must be either Staff or Admin.");
        }
    }
}
