using System.ComponentModel.DataAnnotations;

namespace Brokerage.Web.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(50, ErrorMessage = "Full Name cannot exceed 50 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(
        @"^(?:\+91[\s-]?)?[6-9]\d{9}$",
        ErrorMessage = "Please enter a valid international mobile number.")]
    public string MobileNumber { get; set; } = string.Empty;
    

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}