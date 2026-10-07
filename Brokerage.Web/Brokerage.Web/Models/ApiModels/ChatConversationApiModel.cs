namespace Brokerage.Web.Models.ApiModels;


// Web-side representation of a chat conversation returned by the API.

public class ChatConversationApiModel
{
    public int ConversationID { get; set; }

    public int PropertyID { get; set; }

    public Guid PropertyGUID { get; set; }

    public string PropertyTitle { get; set; } = string.Empty;

    public int BuyerID { get; set; }

    public string BuyerName { get; set; } = string.Empty;

    public int SellerID { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public string? LastMessage { get; set; }

    public DateTime? LastMessageAt { get; set; }

    public int UnreadCount { get; set; }

    public bool IsReadOnly { get; set; }
}