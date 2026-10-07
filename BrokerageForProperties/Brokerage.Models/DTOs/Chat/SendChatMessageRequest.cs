namespace Brokerage.Models.DTOs.Chat;


// Represents a request to send a message within an existing chat conversation.

public class SendChatMessageRequest
{
    public int ConversationID { get; set; }

    public string MessageText { get; set; } = string.Empty;
}