namespace Ecommerce532.API.DTOs.Responses;

public class SuccessResponse
{
    public Guid Guid { get; set; } = new Guid();

    public List<object>? Data { get; set; }

    public string? SuccessNotification { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}
