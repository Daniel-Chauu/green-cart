using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Cart
{
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
