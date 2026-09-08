using System.ComponentModel.DataAnnotations;

namespace Brokerage.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;


    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}