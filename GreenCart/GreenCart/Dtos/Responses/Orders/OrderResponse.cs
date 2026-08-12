using System;
using System.Collections.Generic;

namespace GreenCart.Dtos.Responses.Orders
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderDetailResponse> Items { get; set; } = new();
    }

    public class OrderDetailResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
