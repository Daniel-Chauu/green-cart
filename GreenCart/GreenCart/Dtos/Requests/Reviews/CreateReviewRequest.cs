using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Reviews
{
    public class CreateReviewRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}
