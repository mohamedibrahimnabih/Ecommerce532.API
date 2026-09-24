using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce532.API.DataAccess.EntityConfigurations;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(e => e.TotalPrice).HasPrecision(10, 2);

        builder.Property(e => e.OrderStatus)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(e => e.PaymentMethod)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(e => e.PaymentStatus)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(e => e.CarrierName).IsRequired(false).HasMaxLength(100);
        builder.Property(e => e.TrackingNumber).IsRequired(false).HasMaxLength(100);
        builder.Property(e => e.TransactionId).IsRequired(false).HasMaxLength(200);
        builder.Property(e => e.SessionId).IsRequired(false).HasMaxLength(200);

        builder.HasOne(e => e.ApplicationUser)
               .WithMany()
               .HasForeignKey(e => e.ApplicationUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
