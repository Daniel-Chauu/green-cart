using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Wishlist
{
    public class AddToWishlistRequest
    {
        [Required]
        public int ProductId { get; set; }
    }
}
