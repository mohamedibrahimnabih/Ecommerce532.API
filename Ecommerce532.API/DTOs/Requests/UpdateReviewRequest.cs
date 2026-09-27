namespace Ecommerce532.API.DTOs.Requests;

public record UpdateReviewRequest(int reviewId, int newRate, string newComment);