using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiRequests;

namespace Brokerage.Web.Services;

/// <summary>
/// Provides authenticated Web BFF access to the Chat API.
/// </summary>
public class ChatApiClient
{
    private readonly HttpClient _httpClient;

    public ChatApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient(
                "BrokerageApi");
    }


    /// <summary>
    /// Starts or retrieves a conversation for the authenticated Buyer.
    /// </summary>
    public async Task<ChatConversationApiModel>
        StartConversationAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        StartChatApiRequest request =
            new()
            {
                PropertyGUID = propertyGuid
            };

        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                "api/chat/conversations",
                request,
                cancellationToken);

        return await ReadResponseAsync<ChatConversationApiModel>(
            response,
            cancellationToken);
    }



    // Gets conversations belonging to the authenticated Buyer.

    public async Task<List<ChatConversationApiModel>>
        GetBuyerConversationsAsync(
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/chat/conversations/buyer",
                cancellationToken);

        return await ReadResponseAsync<
            List<ChatConversationApiModel>>(
                response,
                cancellationToken);
    }



    // Gets conversations belonging to the authenticated Seller.

    public async Task<List<ChatConversationApiModel>>
        GetSellerConversationsAsync(
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/chat/conversations/seller",
                cancellationToken);

        return await ReadResponseAsync<
            List<ChatConversationApiModel>>(
                response,
                cancellationToken);
    }



    // Gets one conversation belonging to the authenticated user.

    public async Task<ChatConversationApiModel?>
        GetConversationAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/chat/conversations/{conversationId}",
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadResponseAsync<ChatConversationApiModel>(
            response,
            cancellationToken);
    }



    // Gets persisted messages for a conversation.

    public async Task<List<ChatMessageApiModel>>
        GetMessagesAsync(
            int conversationId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/chat/conversations/{conversationId}/messages",
                cancellationToken);

        return await ReadResponseAsync<
            List<ChatMessageApiModel>>(
                response,
                cancellationToken);
    }


    /// <summary>
    /// Sends a message through the API and returns the persisted message.
    /// </summary>
    public async Task<ChatMessageApiModel>
        SendMessageAsync(
            int conversationId,
            string messageText,
            CancellationToken cancellationToken = default)
    {
        SendChatMessageApiRequest request =
            new()
            {
                ConversationID = conversationId,
                MessageText = messageText
            };

        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                "api/chat/messages",
                request,
                cancellationToken);

        return await ReadResponseAsync<ChatMessageApiModel>(
            response,
            cancellationToken);
    }



    // Marks the current user's received messages in a conversation as read.

    public async Task MarkMessagesAsReadAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/chat/conversations/{conversationId}/read",
                null,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new HttpRequestException(
                error,
                null,
                response.StatusCode);
        }
    }


    private static async Task<T>
        ReadResponseAsync<T>(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new HttpRequestException(
                error,
                null,
                response.StatusCode);
        }

        T? result =
            await response.Content.ReadFromJsonAsync<T>(
                cancellationToken:
                    cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException(
                "API returned an empty response.");
        }

        return result;
    }
}