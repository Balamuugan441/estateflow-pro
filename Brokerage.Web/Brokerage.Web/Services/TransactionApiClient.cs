using Brokerage.Web.Models.ApiRequests;
using Brokerage.Web.Models.ApiResponses;

namespace Brokerage.Web.Services;


/// Provides HTTP access to authenticated Admin transaction operations through the API.

public class TransactionApiClient
{
    private readonly HttpClient _httpClient;

    public TransactionApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient(
                "BrokerageApi");
    }

    public async Task<TransactionListApiResponse>
    GetTransactionsAsync(
        TransactionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        List<string> queryParameters = [];

        queryParameters.Add(
            $"pageNumber={Math.Max(
                request.PageNumber,
                1)}");

        queryParameters.Add(
            $"pageSize={request.PageSize}");

        if (request.FromDate.HasValue)
        {
            queryParameters.Add(
                $"fromDate={request.FromDate.Value:yyyy-MM-dd}");
        }

        if (request.ToDate.HasValue)
        {
            queryParameters.Add(
                $"toDate={request.ToDate.Value:yyyy-MM-dd}");
        }

        if (request.MinPrice.HasValue)
        {
            queryParameters.Add(
                $"minPrice={request.MinPrice.Value}");
        }

        if (request.MaxPrice.HasValue)
        {
            queryParameters.Add(
                $"maxPrice={request.MaxPrice.Value}");
        }

        string queryString =
            string.Join(
                "&",
                queryParameters);

        HttpResponseMessage response =
    await _httpClient.GetAsync(
        $"api/transactions?{queryString}",
        cancellationToken);

        return await ReadResponseAsync<
            TransactionListApiResponse>(
                response,
                cancellationToken);
    }

    public async Task<HttpResponseMessage>
        GetPropertyImageAsync(
            string path,
            CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync(
            path,
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
}