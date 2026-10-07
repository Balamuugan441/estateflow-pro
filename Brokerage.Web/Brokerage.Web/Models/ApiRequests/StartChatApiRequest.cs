namespace Brokerage.Web.Models.ApiRequests
{
    // Request used by the Web BFF to start a Buyer chat.

    public class StartChatApiRequest
    {
        public Guid PropertyGUID { get; set; }
    }
}
