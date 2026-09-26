namespace ECommerce532.API.DTOs.Requests;

public record ProductFilterRequest(string? name, decimal? minPrice, decimal? maxPrice, bool? lessQuantity, int? categoryId, int? brandId);
