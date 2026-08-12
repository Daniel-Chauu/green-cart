using FluentValidation;
using GreenCart.Dtos.Requests.Orders;

namespace GreenCart.Validators.Orders
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required.")
                .MaximumLength(500);

            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required.")
                .MaximumLength(150);

            RuleFor(x => x.RecipientPhone)
                .NotEmpty().WithMessage("Recipient phone is required.")
                .MaximumLength(20);

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required.")
                .MaximumLength(50);

            RuleFor(x => x.VoucherCode)
                .MaximumLength(50)
                .When(x => x.VoucherCode != null);
        }
    }
}
