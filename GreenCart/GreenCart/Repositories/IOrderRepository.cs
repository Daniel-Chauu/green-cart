using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using GreenCart.Repositories.DTOs;
using GreenCart.Repositories.Helpers;

namespace GreenCart.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetOrderByIdWithDetailsAsync(int id);
        Task<Order?> GetOrderByCodeAsync(string orderCode);
        Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(int userId, OrderStatus? status = null);
        Task<PagedResult<Order>> GetFilteredOrdersAsync(OrderFilterParams filterParams);
        Task<PagedResult<Order>> GetOrdersForAdminAsync(GreenCart.Dtos.Requests.Orders.AdminOrderFilterRequest filter);
        Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime startDate, DateTime endDate);
    }
}
