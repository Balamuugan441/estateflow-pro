namespace Brokerage.Models.DTOs.Properties;

public class CreatePropertyResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int PropertyID { get; set; }
}