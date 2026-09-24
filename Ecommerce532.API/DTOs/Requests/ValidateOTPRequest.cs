using System.ComponentModel.DataAnnotations;

namespace ECommerce532.API.DTOs.Requests;

public class ValidateOTPRequest
{
    [Required]
    public string OTP { get; set; } = string.Empty;
}
