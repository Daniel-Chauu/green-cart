using System;
using System.Collections.Generic;

namespace GreenCart.Dtos.Responses.Products
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public double RatingAverage { get; set; }
        public int ReviewCount { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ProductImageResponse> Images { get; set; } = new();
    }

    public class ProductImageResponse
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public string? AltText { get; set; }
    }
}
