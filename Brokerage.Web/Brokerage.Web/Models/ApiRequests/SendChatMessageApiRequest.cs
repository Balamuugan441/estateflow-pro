namespace Brokerage.Web.Models.ApiRequests;


// Request used by the Web BFF to send a chat message.

public class SendChatMessageApiRequest
{
    public int ConversationID { get; set; }

    public string MessageText { get; set; } = string.Empty;
}