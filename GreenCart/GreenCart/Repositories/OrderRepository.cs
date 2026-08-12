using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Data;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using GreenCart.Repositories.DTOs;
using GreenCart.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GreenCart.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Order?> GetOrderByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetOrderByCodeAsync(string orderCode)
        {
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Voucher)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderCode.ToLower() == orderCode.ToLower());
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(int userId, OrderStatus? status = null)
        {
            IQueryable<Order> query = _dbSet
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId);

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<PagedResult<Order>> GetFilteredOrdersAsync(OrderFilterParams filterParams)
        {
            IQueryable<Order> query = _dbSet
                .Include(o => o.User)
                .Include(o => o.OrderDetails);

            if (filterParams.UserId.HasValue)
            {
                query = query.Where(o => o.UserId == filterParams.UserId.Value);
            }

            if (filterParams.Status.HasValue)
            {
                query = query.Where(o => o.Status == filterParams.Status.Value);
            }

            if (filterParams.PaymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == filterParams.PaymentStatus.Value);
            }

            if (filterParams.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= filterParams.FromDate.Value);
            }

            if (filterParams.ToDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= filterParams.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                var term = filterParams.SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderCode.ToLower().Contains(term) ||
                    o.RecipientName.ToLower().Contains(term) ||
                    o.RecipientPhone.ToLower().Contains(term));
            }

            query = query.OrderByDescending(o => o.OrderDate);

            int totalItems = await query.CountAsync();

            var items = await query
                .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalItems, filterParams.PageNumber, filterParams.PageSize);
        }

        public async Task<PagedResult<Order>> GetOrdersForAdminAsync(GreenCart.Dtos.Requests.Orders.AdminOrderFilterRequest filter)
        {
            IQueryable<Order> query = _dbSet
                .Include(o => o.User)
                .Include(o => o.OrderDetails);

            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= filter.ToDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderCode.ToLower().Contains(term) ||
                    o.User.Email.ToLower().Contains(term) ||
                    o.User.FullName.ToLower().Contains(term) ||
                    o.RecipientName.ToLower().Contains(term) ||
                    o.RecipientPhone.ToLower().Contains(term));
            }

            query = query.OrderByDescending(o => o.OrderDate);

            int totalItems = await query.CountAsync();

            var pageIndex = filter.PageIndex <= 0 ? 1 : filter.PageIndex;
            var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Order>(items, totalItems, pageIndex, pageSize);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DateTime startDate, DateTime endDate)
        {
            var ordersQuery = _dbSet.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);

            var totalOrders = await ordersQuery.CountAsync();

            var validOrdersQuery = ordersQuery.Where(o => o.Status != OrderStatus.Cancelled);

            var totalRevenue = await validOrdersQuery.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var totalCustomers = await ordersQuery.Select(o => o.UserId).Distinct().CountAsync();

            var totalProductsSold = await _context.OrderDetails
                .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate && od.Order.Status != OrderStatus.Cancelled)
                .SumAsync(od => (int?)od.Quantity) ?? 0;

            var ordersByStatus = await ordersQuery
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var topProducts = await _context.OrderDetails
                .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate && od.Order.Status != OrderStatus.Cancelled)
                .GroupBy(od => new { od.ProductId, od.ProductName })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.TotalPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalProductsSold = totalProductsSold,
                OrdersByStatus = ordersByStatus,
                TopSellingProducts = topProducts
            };
        }
    }
}
