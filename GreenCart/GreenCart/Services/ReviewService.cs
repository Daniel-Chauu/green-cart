using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Dtos.Requests.Reviews;
using GreenCart.Dtos.Responses.Reviews;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using GreenCart.Repositories;
using GreenCart.Repositories.Helpers;

namespace GreenCart.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ReviewResponse> CreateReviewAsync(int userId, CreateReviewRequest request)
        {
            var deliveredOrders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId, OrderStatus.Delivered);
            var hasPurchasedAndDelivered = deliveredOrders.Any(o =>
                o.OrderDetails.Any(od => od.ProductId == request.ProductId));

            if (!hasPurchasedAndDelivered)
            {
                throw new InvalidOperationException("You can only review products that you have purchased and had delivered.");
            }

         
            var existingReviews = await _unitOfWork.Reviews.FindAsync(
                r => r.UserId == userId && r.ProductId == request.ProductId && !r.IsDeleted);
            if (existingReviews.Any())
            {
                throw new InvalidOperationException("You have already submitted a review for this product.");
            }

            var review = new Review
            {
                UserId = userId,
                ProductId = request.ProductId,
                Rating = request.Rating,
                Comment = request.Comment?.Trim(),
                IsApproved = false 
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            return new ReviewResponse
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                UserName = user?.FullName ?? "Anonymous",
                Rating = review.Rating,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<List<ReviewResponse>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _unitOfWork.Reviews.FindAsync(
                r => r.ProductId == productId && r.IsApproved && !r.IsDeleted);

            var result = new List<ReviewResponse>();
            foreach (var r in reviews)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(r.UserId);
                result.Add(new ReviewResponse
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = user?.FullName ?? "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                });
            }

            return result.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted) return false;

            if (!isAdmin && review.UserId != userId)
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");

            int productId = review.ProductId;
            review.IsDeleted = true;
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            await RecalculateProductRatingAsync(productId);
            return true;
        }

        public async Task<PagedResult<ReviewResponse>> GetAdminReviewsAsync(AdminReviewFilterRequest filter)
        {
            var reviews = (await _unitOfWork.Reviews.FindAsync(r => !r.IsDeleted)).AsQueryable();

            if (filter.ProductId.HasValue)
            {
                reviews = reviews.Where(r => r.ProductId == filter.ProductId.Value);
            }

            if (filter.IsApproved.HasValue)
            {
                reviews = reviews.Where(r => r.IsApproved == filter.IsApproved.Value);
            }

            reviews = reviews.OrderByDescending(r => r.CreatedAt);

            int totalItems = reviews.Count();
            int pageIndex = filter.PageIndex <= 0 ? 1 : filter.PageIndex;
            int pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            var pagedItems = reviews.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            var responseItems = new List<ReviewResponse>();
            foreach (var r in pagedItems)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(r.UserId);
                responseItems.Add(new ReviewResponse
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = user?.FullName ?? "Anonymous",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                });
            }

            return new PagedResult<ReviewResponse>(responseItems, totalItems, pageIndex, pageSize);
        }

        public async Task ApproveReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted)
            {
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");
            }

            review.IsApproved = true;
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            await RecalculateProductRatingAsync(review.ProductId);
        }

        public async Task RejectReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null || review.IsDeleted)
            {
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");
            }

            review.IsApproved = false;
            review.IsDeleted = true; 
            _unitOfWork.Reviews.Update(review);
            await _unitOfWork.SaveChangesAsync();

            await RecalculateProductRatingAsync(review.ProductId);
        }

        private async Task RecalculateProductRatingAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null) return;

            var productReviews = await _unitOfWork.Reviews.FindAsync(
                r => r.ProductId == productId && r.IsApproved && !r.IsDeleted);

            if (productReviews.Any())
            {
                product.RatingAverage = Math.Round(productReviews.Average(r => r.Rating), 1);
                product.ReviewCount = productReviews.Count;
            }
            else
            {
                product.RatingAverage = 0.0;
                product.ReviewCount = 0;
            }

            _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
