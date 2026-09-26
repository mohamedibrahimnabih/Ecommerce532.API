namespace Ecommerce532.API.DTOs.Responses;

public class ProductWithRelatedResponse
{
    public Product Product { get; set; } = null!;
    public IEnumerable<Product> RelatedProducts { get; set; } = [];
}
