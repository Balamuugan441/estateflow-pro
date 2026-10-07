using Brokerage.Models.DTOs.Locations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Brokerage.Business.Services;

/// <summary>
/// Provides searchable Indian state and city reference data.
/// </summary>
/// <remarks>
/// Called by LocationController in the API layer.
///
/// The service uses CountriesNow as the external location provider,
/// caches the India location dataset, and performs prefix matching
/// against the cached data.
/// </remarks>
public sealed class IndiaLocationService : ILocationService
{
    private const string CacheKey =
        "estateflow:locations:india";

    private const int MaximumResults =
        25;

    private readonly HttpClient _httpClient;

    private readonly IMemoryCache _memoryCache;

    private readonly ILogger<IndiaLocationService> _logger;

    public IndiaLocationService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        ILogger<IndiaLocationService> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Searches Indian states and cities using a prefix.
    /// </summary>
    /// <param name="search">The search prefix.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the external request.
    /// </param>
    /// <returns>Matching Indian states and cities.</returns>
    public async Task<IReadOnlyList<LocationSearchOptionDto>>
        SearchIndiaAsync(
            string? search,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyList<LocationSearchOptionDto> locations =
            await GetIndiaLocationsAsync(
                cancellationToken);

        string? searchTerm =
            string.IsNullOrWhiteSpace(search)
                ? null
                : search.Trim();

        IEnumerable<LocationSearchOptionDto> results =
            string.IsNullOrWhiteSpace(searchTerm)
                ? locations
                : locations.Where(location =>
                    location.Name.StartsWith(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase));

        return results
            .OrderBy(location =>
                location.Type == "State"
                    ? 0
                    : 1)
            .ThenBy(location => location.Name)
            .Take(MaximumResults)
            .ToList();
    }

    private async Task<IReadOnlyList<LocationSearchOptionDto>>
        GetIndiaLocationsAsync(
            CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(
            CacheKey,
            out IReadOnlyList<LocationSearchOptionDto>? cachedLocations))
        {
            return cachedLocations;
        }

        try
        {
            CountriesNowStatesResponse? statesResponse =
                await _httpClient.GetFromJsonAsync<CountriesNowStatesResponse>(
                    "countries/states/q?country=India",
                    cancellationToken);

            CountriesNowCitiesResponse? citiesResponse =
                await _httpClient.GetFromJsonAsync<CountriesNowCitiesResponse>(
                    "countries/cities/q?country=India",
                    cancellationToken);

            if (statesResponse == null ||
                statesResponse.Error ||
                statesResponse.Data?.States == null)
            {
                throw new HttpRequestException(
                    "Unable to retrieve Indian states.");
            }

            if (citiesResponse == null ||
                citiesResponse.Error ||
                citiesResponse.Data == null)
            {
                throw new HttpRequestException(
                    "Unable to retrieve Indian cities.");
            }

            List<LocationSearchOptionDto> locations =
                statesResponse.Data.States
                    .Select(state => new LocationSearchOptionDto
                    {
                        Type = "State",
                        Name = state.Name,
                        Country = "India"
                    })
                    .Concat(
                        citiesResponse.Data.Select(city =>
                            new LocationSearchOptionDto
                            {
                                Type = "City",
                                Name = city,
                                Country = "India"
                            }))
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.Name))
                    .GroupBy(
                        x => $"{x.Type}:{x.Name}",
                        StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First())
                    .OrderBy(x => x.Type == "State" ? 0 : 1)
                    .ThenBy(x => x.Name)
                    .ToList();

            _memoryCache.Set(
                CacheKey,
                locations,
                TimeSpan.FromHours(12));

            return locations;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Unable to load Indian state and city data.");

            throw;
        }
    }

    private sealed class CountriesNowStatesResponse
    {
        public bool Error { get; set; }

        public CountriesNowStatesData? Data { get; set; }
    }

    private sealed class CountriesNowStatesData
    {
        public List<CountriesNowState> States { get; set; } = [];
    }

    private sealed class CountriesNowState
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class CountriesNowCitiesResponse
    {
        public bool Error { get; set; }

        public List<string>? Data { get; set; }
    }
}