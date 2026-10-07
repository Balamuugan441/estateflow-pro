namespace Brokerage.Web.Models.ApiResponses;

public class ForgotPasswordResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}