using System.ComponentModel.DataAnnotations;

namespace Brokerage.Web.Models
{
    public class ResetPasswordInput
    {
        [Required(ErrorMessage = "New password is required.")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage =
                "Password must be between 8 and 100 characters.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage =
                "Password must contain at least one uppercase letter, one lowercase letter, and one special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare(
            "NewPassword",
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
