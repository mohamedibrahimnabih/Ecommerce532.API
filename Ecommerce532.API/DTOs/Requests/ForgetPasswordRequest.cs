using System.ComponentModel.DataAnnotations;

namespace ECommerce532.API.DTOs.Requests;

public class ForgetPasswordRequest
{
    [Required]
    [Display(Name = "Email Or UserName")]
    public string EmailOrUserName { get; set; } = string.Empty;
}
