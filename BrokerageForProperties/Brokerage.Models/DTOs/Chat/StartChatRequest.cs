namespace Brokerage.Models.DTOs.Chat;


// Represents a request to open or create a Buyer chat for a specific property.

public class StartChatRequest
{
    public Guid PropertyGUID { get; set; }
}