using System;

namespace GreenCart.Dtos.Responses.Wishlist
{
    public class WishlistItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
