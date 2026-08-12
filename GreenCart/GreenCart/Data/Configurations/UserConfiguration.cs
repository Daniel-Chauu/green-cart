using GreenCart.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenCart.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(u => u.Address)
                .HasMaxLength(500);

            builder.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(u => u.RefreshToken)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(u => u.RefreshToken)
                .IsUnique()
                .HasFilter("[RefreshToken] IS NOT NULL");

            // Ignore computed property (alias for RefreshTokenExpiryTime)
            builder.Ignore(u => u.RefreshTokenExpiry);

            builder.Property(u => u.ResetToken)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(u => u.ResetTokenExpiry)
                .IsRequired(false);

            builder.Property(u => u.FailedResetAttempts)
                .HasDefaultValue(0);
        }
    }
}
