using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Orders;
using GreenCart.Dtos.Responses.Orders;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using GreenCart.Repositories;
using Microsoft.Extensions.Logging;

namespace GreenCart.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<OrderResponse> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            var cartItems = await _unitOfWork.CartItems.FindAsync(c => c.UserId == userId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Your cart is empty.");

            Voucher? voucher = null;
            decimal discountAmount = 0;

            if (!string.IsNullOrWhiteSpace(request.VoucherCode))
            {
                var vouchers = await _unitOfWork.Vouchers.FindAsync(
                    v => v.Code.ToLower() == request.VoucherCode.Trim().ToLower());
                voucher = vouchers.FirstOrDefault();

                if (voucher == null)
                    throw new KeyNotFoundException("Voucher not found.");

                if (!voucher.IsActive || voucher.StartDate > DateTime.UtcNow || voucher.EndDate < DateTime.UtcNow)
                    throw new InvalidOperationException("Voucher is expired or inactive.");

                if (voucher.TimesUsed >= voucher.UsageLimit)
                    throw new InvalidOperationException("Voucher usage limit has been reached.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var orderDetails = new List<OrderDetail>();
                decimal subTotal = 0;

                foreach (var cartItem in cartItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId);
                    if (product == null || !product.IsActive)
                        throw new InvalidOperationException($"Product '{cartItem.ProductId}' is no longer available.");

                    var inventory = await GetInventoryByProductIdAsync(product.Id);
                    int availableStock = inventory != null
                        ? inventory.Quantity - inventory.ReservedQuantity
                        : product.StockQuantity;

                    if (availableStock < cartItem.Quantity)
                        throw new InvalidOperationException(
                            $"Insufficient stock for '{product.Name}'. Available: {availableStock}, Requested: {cartItem.Quantity}");

                    var unitPrice = product.SalePrice ?? product.BasePrice;
                    var detail = new OrderDetail
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        UnitPrice = unitPrice,
                        Quantity = cartItem.Quantity,
                        TotalPrice = unitPrice * cartItem.Quantity
                    };
                    orderDetails.Add(detail);
                    subTotal += detail.TotalPrice;

                    if (inventory != null)
                    {
                        inventory.Quantity -= cartItem.Quantity;
                        _unitOfWork.Inventories.Update(inventory);
                    }
                    product.StockQuantity -= cartItem.Quantity;
                    _unitOfWork.Products.Update(product);
                }

                if (voucher != null)
                {
                    if (subTotal < voucher.MinimumOrderAmount)
                        throw new InvalidOperationException(
                            $"Minimum order amount for this voucher is {voucher.MinimumOrderAmount:C}.");

                    if (voucher.DiscountType == "Percentage")
                    {
                        discountAmount = subTotal * (voucher.DiscountValue / 100);
                        if (voucher.MaximumDiscountAmount.HasValue)
                            discountAmount = Math.Min(discountAmount, voucher.MaximumDiscountAmount.Value);
                    }
                    else
                    {
                        discountAmount = voucher.DiscountValue;
                    }

                    voucher.TimesUsed++;
                    _unitOfWork.Vouchers.Update(voucher);
                }

                var order = new Order
                {
                    OrderCode = $"GC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
                    UserId = userId,
                    VoucherId = voucher?.Id,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    PaymentStatus = PaymentStatus.Pending,
                    ShippingAddress = request.ShippingAddress.Trim(),
                    RecipientName = request.RecipientName.Trim(),
                    RecipientPhone = request.RecipientPhone.Trim(),
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    ShippingFee = 0,
                    TotalAmount = subTotal - discountAmount,
                    Note = request.Note?.Trim(),
                    PaymentMethod = request.PaymentMethod
                };

                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                foreach (var detail in orderDetails)
                {
                    detail.OrderId = order.Id;
                    await _unitOfWork.OrderDetails.AddAsync(detail);
                }

                foreach (var cartItem in cartItems)
                {
                    _unitOfWork.CartItems.Delete(cartItem);
                }

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return MapToResponse(order, orderDetails);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(int orderId, int userId)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdWithDetailsAsync(orderId);
            if (order == null || order.UserId != userId)
                return null;

            return MapToResponse(order, order.OrderDetails.ToList());
        }

        public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
        {
            var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
            return orders.Select(o => MapToResponse(o, o.OrderDetails.ToList())).ToList();
        }

        public async Task<OrderResponse> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdWithDetailsAsync(orderId)
                ?? throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            order.Status = request.Status;
            if (request.PaymentStatus.HasValue)
                order.PaymentStatus = request.PaymentStatus.Value;

            
            if (request.Status == OrderStatus.Delivered && order.PaymentMethod == "COD")
            {
                order.PaymentStatus = PaymentStatus.Paid;
                _logger.LogInformation(
                    "COD order {OrderId} (Code: {OrderCode}) auto-marked as Paid on delivery.",
                    orderId, order.OrderCode);
            }

           
            if (request.Status == OrderStatus.Cancelled && order.PaymentStatus == PaymentStatus.Pending)
            {
                order.PaymentStatus = PaymentStatus.Refunded;
                _logger.LogInformation(
                    "Order {OrderId} (Code: {OrderCode}) auto-marked as Refunded on cancellation.",
                    orderId, order.OrderCode);
            }

            order.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} status updated to {Status}, payment status: {PaymentStatus}.",
                orderId, order.Status, order.PaymentStatus);

            return MapToResponse(order, order.OrderDetails.ToList());
        }

        public async Task<GreenCart.Repositories.Helpers.PagedResult<OrderResponse>> GetAdminOrdersAsync(AdminOrderFilterRequest filter)
        {
            var pagedOrders = await _unitOfWork.Orders.GetOrdersForAdminAsync(filter);
            var responses = pagedOrders.Items.Select(o => MapToResponse(o, o.OrderDetails?.ToList() ?? new List<OrderDetail>())).ToList();
            return new GreenCart.Repositories.Helpers.PagedResult<OrderResponse>(responses, pagedOrders.TotalItems, pagedOrders.PageNumber, pagedOrders.PageSize);
        }

        private async Task<Inventory?> GetInventoryByProductIdAsync(int productId)
        {
            var inventories = await _unitOfWork.Inventories.FindAsync(i => i.ProductId == productId);
            return inventories.FirstOrDefault();
        }

        private static OrderResponse MapToResponse(Order order, List<OrderDetail> details)
        {
            return new OrderResponse
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                Status = order.Status.ToString(),
                PaymentStatus = order.PaymentStatus.ToString(),
                ShippingAddress = order.ShippingAddress,
                RecipientName = order.RecipientName,
                RecipientPhone = order.RecipientPhone,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                ShippingFee = order.ShippingFee,
                TotalAmount = order.TotalAmount,
                Note = order.Note,
                PaymentMethod = order.PaymentMethod,
                OrderDate = order.OrderDate,
                CreatedAt = order.CreatedAt,
                Items = details.Select(d => new OrderDetailResponse
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity,
                    TotalPrice = d.TotalPrice
                }).ToList()
            };
        }
    }
}
