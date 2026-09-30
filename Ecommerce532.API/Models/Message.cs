namespace Ecommerce532.API.Models;

public class Message
{
    public int Id { get; set; }
    public string ToUserId { get; set; }
    public string FromUserId { get; set; }
    public string Message { get; set; }
    public DateTime DateTime { get; set; } = DateTime.Now;
}
