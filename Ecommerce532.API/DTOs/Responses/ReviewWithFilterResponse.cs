using Ecommerce532.API.Models;

namespace Ecommerce532.API.DTOs.Responses;

public class ReviewWithFilterResponse
{
    public IEnumerable<UserProductReview> Reviews { get; set; } = new List<UserProductReview>();

    public string Query { get; set; } = string.Empty;
    public double TotalPages { get; set; }
    public int CurrentPage { get; set; }
}
