using System;

namespace GreenCart.Entities
{
    public class Inventory : BaseEntity
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; } = 0;
        public string? Location { get; set; }
        public int MinimumStockLevel { get; set; } = 10;
        public int ReorderQuantity { get; set; } = 50;
        public DateTime? LastRestockedAt { get; set; }

        // Navigation property
        public virtual Product Product { get; set; } = null!;
    }
}
