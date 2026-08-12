using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Wishlist
{
    public class AddToWishlistRequest
    {
        public int ProductId { get; set; }
    }
}
