using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce532.API.DataAccess.EntityConfigurations;

public class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(e => e.Price).HasPrecision(10, 2);

        builder.HasOne(e => e.Order)
               .WithMany()
               .HasForeignKey(e => e.OrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
