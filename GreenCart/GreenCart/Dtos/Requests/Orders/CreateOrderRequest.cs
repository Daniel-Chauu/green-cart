using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        public string RecipientPhone { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string PaymentMethod { get; set; } = "COD";

        public string? VoucherCode { get; set; }
    }
}
