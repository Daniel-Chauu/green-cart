using System;

namespace GreenCart.Entities
{
    public class ShippingAddress : BaseEntity
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Vietnam";
        public bool IsDefault { get; set; } = false;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
