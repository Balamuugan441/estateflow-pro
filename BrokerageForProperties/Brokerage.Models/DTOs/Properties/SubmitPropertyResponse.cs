namespace Brokerage.Models.DTOs.Properties;

public class SubmitPropertyResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = [];
}