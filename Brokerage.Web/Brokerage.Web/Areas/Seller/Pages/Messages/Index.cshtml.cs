using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Seller.Pages.Messages;

public class IndexModel : PageModel
{
    private readonly ChatApiClient _chatApiClient;

    public IndexModel(ChatApiClient chatApiClient)
    {
        _chatApiClient = chatApiClient;
    }

    public List<ChatConversationApiModel> Conversations { get; private set; } = [];

    public ChatConversationApiModel? ActiveConversation { get; private set; }

    public List<ChatMessageApiModel> Messages { get; private set; } = [];

    public int? ConversationId { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int? conversationId,
        CancellationToken cancellationToken)
    {
        ConversationId = conversationId;

        Conversations =
            await _chatApiClient.GetSellerConversationsAsync(
                cancellationToken);

        if (conversationId.HasValue)
        {
            ActiveConversation =
                await _chatApiClient.GetConversationAsync(
                    conversationId.Value,
                    cancellationToken);

            if (ActiveConversation is null)
            {
                return NotFound();
            }

            Messages =
                await _chatApiClient.GetMessagesAsync(
                    conversationId.Value,
                    cancellationToken);
        }

        return Page();
    }
}