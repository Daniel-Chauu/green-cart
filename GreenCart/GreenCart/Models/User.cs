using System;
using System.Collections.Generic;
using GreenCart.Entities.Enums;

namespace GreenCart.Entities
{
    public class User : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public DateTime? RefreshTokenExpiry
        {
            get => RefreshTokenExpiryTime;
            set => RefreshTokenExpiryTime = value;
        }

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }
        public int FailedResetAttempts { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
