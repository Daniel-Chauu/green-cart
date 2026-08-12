namespace GreenCart.Repositories.Helpers
{
    public class ProductFilterParams
    {
        public string? SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// Options: price_asc, price_desc, name_asc, name_desc, rating, newest
        /// </summary>
        public string? SortBy { get; set; } = "newest";

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
