using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Reviews
{
    public class CreateReviewRequest
    {
        public int ProductId { get; set; }
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
