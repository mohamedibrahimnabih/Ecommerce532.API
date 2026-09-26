namespace Ecommerce532.API.DTOs.Responses;

public class CategoryWithFilterResponse
{
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();

    public string Query { get; set; } = string.Empty;
    public double TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
