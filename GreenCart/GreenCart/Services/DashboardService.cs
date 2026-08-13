using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Responses.Dashboard;
using GreenCart.Entities.Enums;
using GreenCart.Repositories;

namespace GreenCart.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DashboardSummaryResponse> GetSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var ordersToday = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= today && o.OrderDate < tomorrow);

            var totalRevenueToday = ordersToday
                .Where(o => o.Status != OrderStatus.Cancelled)
                .Sum(o => o.TotalAmount);

            var totalUsers = await _unitOfWork.Users.CountAsync();

            var allOrders = await _unitOfWork.Orders.GetAllAsync();
            var ordersByStatus = allOrders
                .GroupBy(o => o.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            var topSelling = await GetTopSellingProductsAsync(5);

            return new DashboardSummaryResponse
            {
                TotalRevenueToday = totalRevenueToday,
                TotalOrdersToday = ordersToday.Count,
                TotalUsers = totalUsers,
                OrdersByStatus = ordersByStatus,
                TopSellingProducts = topSelling
            };
        }

        public async Task<List<TopSellingProductResponse>> GetTopSellingProductsAsync(int count = 5)
        {
            var orderDetails = await _unitOfWork.OrderDetails.GetAllAsync();
            var nonCancelledOrders = await _unitOfWork.Orders.FindAsync(o => o.Status != OrderStatus.Cancelled);
            var validOrderIds = new HashSet<int>(nonCancelledOrders.Select(o => o.Id));

            var topProducts = orderDetails
                .Where(od => validOrderIds.Contains(od.OrderId))
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new TopSellingProductResponse
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(count)
                .ToList();

            return topProducts;
        }
    }
}
