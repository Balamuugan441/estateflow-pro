namespace Brokerage.Models.DTOs.Authentication;

public class RegisterResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}