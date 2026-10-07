using Brokerage.Models.DTOs.Locations;

namespace Brokerage.Business.Services;

/// <summary>
/// Provides searchable Indian state and city reference data.
/// </summary>
/// <remarks>
/// The service retrieves location data from the external CountriesNow
/// provider and caches the normalized result for subsequent searches.
/// </remarks>
public interface ILocationService
{
    /// <summary>
    /// Searches Indian states and cities using a prefix.
    /// </summary>
    /// <param name="search">The search prefix.</param>
    /// <param name="cancellationToken">
    /// Token used to cancel the external request.
    /// </param>
    /// <returns>Matching Indian states and cities.</returns>
    Task<IReadOnlyList<LocationSearchOptionDto>> SearchIndiaAsync(
        string? search,
        CancellationToken cancellationToken = default);
}