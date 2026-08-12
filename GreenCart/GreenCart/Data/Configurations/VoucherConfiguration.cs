using GreenCart.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenCart.Data.Configurations
{
    public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
    {
        public void Configure(EntityTypeBuilder<Voucher> builder)
        {
            builder.ToTable("Vouchers");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(v => v.Code)
                .IsUnique();

            builder.Property(v => v.Description)
                .HasMaxLength(250);

            builder.Property(v => v.DiscountType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.DiscountValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(v => v.MinimumOrderAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(v => v.MaximumDiscountAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
