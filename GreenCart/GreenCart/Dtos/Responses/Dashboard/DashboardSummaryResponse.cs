using System.Collections.Generic;

namespace GreenCart.Dtos.Responses.Dashboard
{
    public class DashboardSummaryResponse
    {
        public decimal TotalRevenueToday { get; set; }
        public int TotalOrdersToday { get; set; }
        public int TotalUsers { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; } = new();
        public List<TopSellingProductResponse> TopSellingProducts { get; set; } = new();
    }

    public class TopSellingProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
