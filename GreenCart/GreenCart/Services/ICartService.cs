using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Cart;
using GreenCart.Dtos.Responses.Cart;

namespace GreenCart.Services
{
    public interface ICartService
    {
        Task<CartResponse> GetCartAsync(int userId);
        Task<CartItemResponse> AddToCartAsync(int userId, AddToCartRequest request);
        Task<CartItemResponse> UpdateCartItemAsync(int userId, int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
    }
}
