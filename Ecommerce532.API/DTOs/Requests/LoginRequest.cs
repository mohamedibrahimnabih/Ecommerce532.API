using System.ComponentModel.DataAnnotations;

namespace ECommerce532.API.DTOs.Requests;

public class LoginRequest
{
    [Required]
    [Display(Name = "Email Or UserName")]
    public string EmailOrUserName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool Remember { get; set; }
}
