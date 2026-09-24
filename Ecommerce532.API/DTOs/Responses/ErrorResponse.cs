namespace Ecommerce532.API.DTOs.Responses;

public class ErrorResponse
{
    public Guid Guid { get; set; } = new Guid();

    public List<object>? Data { get; set; }

    public string? ErrorNotification { get; set; }

    public DateTime DateTime { get; set; } = DateTime.UtcNow;
}
