namespace Brokerage.Models.DTOs.Authentication;

public class ForgotPasswordResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}