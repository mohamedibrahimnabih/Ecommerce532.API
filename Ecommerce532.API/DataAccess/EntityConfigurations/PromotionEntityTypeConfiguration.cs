using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce532.API.DataAccess.EntityConfigurations;

public class PromotionEntityTypeConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Discount).HasPrecision(5, 2);

        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasOne(e => e.Product)
               .WithMany()
               .HasForeignKey(e => e.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
