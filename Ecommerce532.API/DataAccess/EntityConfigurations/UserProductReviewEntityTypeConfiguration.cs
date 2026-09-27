using Ecommerce532.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce532.API.DataAccess.EntityConfigurations;

public class UserProductReviewEntityTypeConfiguration : IEntityTypeConfiguration<UserProductReview>
{
    public void Configure(EntityTypeBuilder<UserProductReview> builder)
    {
        builder.Property(e => e.Rate)
               .IsRequired();

        builder.Property(e => e.Comment)
               .HasMaxLength(1000);

        builder.Property(e => e.Reply)
               .HasMaxLength(1000);

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
