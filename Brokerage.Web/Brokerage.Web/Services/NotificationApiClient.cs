using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;

namespace Brokerage.Web.Services;

// Provides HTTP access to authenticated user notifications through the API.

public class NotificationApiClient
{
    private readonly HttpClient _httpClient;

    public NotificationApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient(
                "BrokerageApi");
    }

    public async Task<List<NotificationApiModel>>
        GetUnreadAsync(
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/notifications/unread",
                cancellationToken);

        return await ReadResponseAsync<
            List<NotificationApiModel>>(
                response,
                cancellationToken);
    }

    public async Task MarkAsReadAsync(
        int notificationId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/notifications/{notificationId}/read",
                null,
                cancellationToken);

        await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
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
    public async Task<List<NotificationApiModel>>
    GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/notifications",
                cancellationToken);

        return await ReadResponseAsync<
            List<NotificationApiModel>>(
                response,
                cancellationToken);
    }
}