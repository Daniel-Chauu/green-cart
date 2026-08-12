using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GreenCart.Dtos.Requests.Products
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }

        public decimal? SalePrice { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }

        public int? BrandId { get; set; }

        public bool IsActive { get; set; } = true;

        public List<IFormFile>? Images { get; set; }
    }
}
