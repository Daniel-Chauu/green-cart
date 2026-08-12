using System.ComponentModel.DataAnnotations;
using GreenCart.Entities.Enums;

namespace GreenCart.Dtos.Requests.Orders
{
    public class UpdateOrderStatusRequest
    {
        [Required]
        public OrderStatus Status { get; set; }

        public PaymentStatus? PaymentStatus { get; set; }
    }
}
