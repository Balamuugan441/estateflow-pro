namespace Brokerage.Models.DTOs.Chat;

/// Provides message information required to display a persisted chat message.

public class ChatMessageDto
{
    public int MessageID { get; set; }

    public int ConversationID { get; set; }

    public int PropertyID { get; set; }

    public int SenderID { get; set; }

    public string SenderName { get; set; } = string.Empty;

    public int ReceiverID { get; set; }

    public string ReceiverName { get; set; } = string.Empty;

    public string MessageText { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }
}