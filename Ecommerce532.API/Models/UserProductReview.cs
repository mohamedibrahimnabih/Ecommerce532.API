namespace Ecommerce532.API.Models;

public class UserProductReview
{
    public int Id { get; set; }
    public string ApplicationUserId { get; set; } = string.Empty;
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Rate { get; set; }
    public string? Comment { get; set; }
    public string? Reply { get; set; }
}
