using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Payments
{
    public class CreateVnPayPaymentRequest
    {
        public int OrderId { get; set; }
    }
}
