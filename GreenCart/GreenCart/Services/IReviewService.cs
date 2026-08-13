using System.Collections.Generic;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Reviews;
using GreenCart.Dtos.Responses.Reviews;
using GreenCart.Repositories.Helpers;

namespace GreenCart.Services
{
    public interface IReviewService
    {
        Task<ReviewResponse> CreateReviewAsync(int userId, CreateReviewRequest request);
        Task<List<ReviewResponse>> GetProductReviewsAsync(int productId);
        Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false);
        Task<PagedResult<ReviewResponse>> GetAdminReviewsAsync(AdminReviewFilterRequest filter);
        Task ApproveReviewAsync(int reviewId);
        Task RejectReviewAsync(int reviewId);
    }
}
