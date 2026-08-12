using GreenCart.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenCart.Data.Configurations
{
    public class ShippingAddressConfiguration : IEntityTypeConfiguration<ShippingAddress>
    {
        public void Configure(EntityTypeBuilder<ShippingAddress> builder)
        {
            builder.ToTable("ShippingAddresses");

            builder.HasKey(sa => sa.Id);

            builder.Property(sa => sa.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sa => sa.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sa => sa.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(sa => sa.AddressLine2)
                .HasMaxLength(200)
                .IsRequired(false);

            builder.Property(sa => sa.City)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sa => sa.State)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sa => sa.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(sa => sa.Country)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(sa => sa.User)
                .WithMany()
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
