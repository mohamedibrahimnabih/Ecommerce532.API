using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce532.API.Models;

public class ReviewImg
{
    public int Id { get; set; }

    public int UserProductReviewId { get; set; }
    //[ForeignKey(nameof(ReviewId))]
    public UserProductReview UserProductReview { get; set; } = null!;

    public string Img { get; set; } = string.Empty;
}
