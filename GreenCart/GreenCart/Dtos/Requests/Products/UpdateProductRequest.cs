using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GreenCart.Dtos.Requests.Products
{
    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public decimal? BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public int? StockQuantity { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool? IsActive { get; set; }

        public List<IFormFile>? NewImages { get; set; }
        public List<int>? RemoveImageIds { get; set; }
    }
}
