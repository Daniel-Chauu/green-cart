using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Responses.Wishlist;

namespace GreenCart.Services
{
    public interface IWishlistService
    {
        Task<List<WishlistItemResponse>> GetUserWishlistAsync(int userId);
        Task<WishlistItemResponse> AddToWishlistAsync(int userId, int productId);
        Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    }
}
