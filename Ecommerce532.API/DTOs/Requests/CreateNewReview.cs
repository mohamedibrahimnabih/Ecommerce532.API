namespace Ecommerce532.API.DTOs.Requests;

public record CreateNewReview(int rate, int productId, string? comment);