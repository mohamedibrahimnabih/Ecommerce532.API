namespace Ecommerce532.API.DTOs.Responses;

public class BrandWithFilterResponse
{
    public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();

    public string Query { get; set; } = string.Empty;
    public double TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
