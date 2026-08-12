using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Orders;
using GreenCart.Dtos.Responses.Orders;

namespace GreenCart.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request);
        Task<OrderResponse?> GetOrderByIdAsync(int orderId, int userId);
        Task<List<OrderResponse>> GetUserOrdersAsync(int userId);
        Task<OrderResponse> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);
        Task<GreenCart.Repositories.Helpers.PagedResult<OrderResponse>> GetAdminOrdersAsync(AdminOrderFilterRequest filter);
    }
}
