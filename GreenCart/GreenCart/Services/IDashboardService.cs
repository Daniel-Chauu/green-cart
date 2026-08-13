using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Responses.Dashboard;

namespace GreenCart.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryResponse> GetSummaryAsync();
        Task<List<TopSellingProductResponse>> GetTopSellingProductsAsync(int count = 5);
    }
}
