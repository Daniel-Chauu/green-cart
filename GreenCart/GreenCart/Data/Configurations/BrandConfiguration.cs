using GreenCart.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenCart.Data.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.Slug)
                .IsRequired()
                .HasMaxLength(120);

            builder.HasIndex(b => b.Slug)
                .IsUnique();

            builder.Property(b => b.Description)
                .HasMaxLength(500);

            builder.Property(b => b.LogoUrl)
                .HasMaxLength(500);

            builder.Property(b => b.Origin)
                .HasMaxLength(100);
        }
    }
}
