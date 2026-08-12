using System;
using GreenCart.Entities.Enums;

namespace GreenCart.Dtos.Requests.Orders
{
    public class AdminOrderFilterRequest
    {
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
