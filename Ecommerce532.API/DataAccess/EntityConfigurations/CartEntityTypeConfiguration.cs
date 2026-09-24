using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce532.API.DataAccess.EntityConfigurations;

public class CartEntityTypeConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.Property(e => e.CurrentPrice).HasPrecision(10, 2);

        builder.HasOne(e => e.ApplicationUser)
               .WithMany()
               .HasForeignKey(e => e.ApplicationUserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
