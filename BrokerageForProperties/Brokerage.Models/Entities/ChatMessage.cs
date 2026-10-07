namespace Brokerage.Models.Entities;


/// Represents a persisted message exchanged within a property-specific chat conversation.

public class ChatMessage
{
    public int MessageID { get; set; }

    public int ConversationID { get; set; }

    public int SenderID { get; set; }

    public int ReceiverID { get; set; }

    public string MessageText { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }
}