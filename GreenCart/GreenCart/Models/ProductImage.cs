namespace GreenCart.Entities
{
    public class ProductImage : BaseEntity
    {
        public int ProductId { get; set; }
        /// <summary>
        /// Stores relative path, e.g., /images/products/1/abc.jpg
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public string? AltText { get; set; }

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}
