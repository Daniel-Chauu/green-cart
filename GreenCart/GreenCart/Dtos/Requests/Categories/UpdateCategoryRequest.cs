using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Categories
{
    public class UpdateCategoryRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
