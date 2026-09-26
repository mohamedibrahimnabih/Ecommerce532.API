namespace Ecommerce532.API.DTOs.Requests;

public class BrandCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public IFormFile Img { get; set; } = null!;
    public bool Status { get; set; }
}
