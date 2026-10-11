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

        if (!response.IsSuccessStatusCode)
        {
            string errorBody =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new HttpRequestException(
                $"Home API failed. " +
                $"Status: {(int)response.StatusCode} " +
                $"{response.StatusCode}. " +
                $"Response: {errorBody}",
                null,
                response.StatusCode);
        }

        return await response.Content
            .ReadFromJsonAsync<List<PublicPropertyApiModel>>(
                cancellationToken)
            ?? [];
    }
    public async Task<List<PublicPropertyApiModel>>
   GetLatestPropertiesAsync(
       string? city,
       int? bedrooms,
       int? bathrooms,
       string? listingType,
       decimal? minPrice,
       decimal? maxPrice,
       CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(city))
            query.Add($"city={Uri.EscapeDataString(city.Trim())}");

        if (bedrooms.HasValue)
            query.Add($"bedrooms={bedrooms.Value}");

        if (bathrooms.HasValue)
            query.Add($"bathrooms={bathrooms.Value}");

        if (!string.IsNullOrWhiteSpace(listingType))
            query.Add($"listingType={Uri.EscapeDataString(listingType)}");

        if (minPrice.HasValue)
            query.Add($"minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        if (maxPrice.HasValue)
            query.Add($"maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");

        string url = "api/Home/properties";

        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        using HttpResponseMessage response =
    await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PublicPropertyApiModel>>(
                cancellationToken)
            ?? [];
    }

}