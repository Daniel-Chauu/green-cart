using GreenCart.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenCart.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.OrderCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(o => o.OrderCode)
                .IsUnique();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.PaymentStatus)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.RecipientName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(o => o.RecipientPhone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(o => o.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.ShippingFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.PaymentMethod)
                .HasMaxLength(50);

            builder.Property(o => o.Note)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
