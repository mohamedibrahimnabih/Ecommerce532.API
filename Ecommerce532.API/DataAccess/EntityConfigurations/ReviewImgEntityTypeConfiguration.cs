using Ecommerce532.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce532.API.DataAccess.EntityConfigurations;

public class ReviewImgEntityTypeConfiguration : IEntityTypeConfiguration<ReviewImg>
{
    public void Configure(EntityTypeBuilder<ReviewImg> builder)
    {
        builder.Property(e => e.Img)
               .IsRequired()
               .HasMaxLength(500);

        builder.HasOne(e => e.UserProductReview)
               .WithMany()
               .HasForeignKey(e => e.UserProductReviewId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
