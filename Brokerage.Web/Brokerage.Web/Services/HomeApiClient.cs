using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Services;

public class HomeApiClient
{
    private readonly HttpClient _httpClient;

    public HomeApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("BrokeragePublicApi");

    }

    public async Task<PublicHomeStatsApiModel> GetStatsAsync(CancellationToken cancellationToken = default)

    {
        HttpResponseMessage response = await _httpClient.GetAsync("api/Home/stats", cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<PublicHomeStatsApiModel>(cancellationToken) ?? new PublicHomeStatsApiModel();

    }

    public async Task<List<PublicPropertyApiModel>>
        GetLatestPropertiesAsync(string? city, CancellationToken cancellationToken = default)

    {
        string url =
            string.IsNullOrWhiteSpace(city)
                ? "api/Home/properties"
                : $"api/Home/properties?city={Uri.EscapeDataString(city)}";

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PublicPropertyApiModel>>(
                cancellationToken)
            ?? [];
    }
}