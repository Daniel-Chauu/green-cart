using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Payments
{
    public class CreateVnPayPaymentRequest
    {
        [Required]
        public int OrderId { get; set; }
    }
}
