using System.Collections.Generic;

namespace GreenCart.Dtos.Responses.Categories
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public List<CategoryResponse> SubCategories { get; set; } = new List<CategoryResponse>();
    }
}
