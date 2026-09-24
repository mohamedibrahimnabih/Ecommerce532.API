using System.ComponentModel.DataAnnotations;

namespace ECommerce532.API.DTOs.Requests;

public class NewPasswordRequest
{
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
