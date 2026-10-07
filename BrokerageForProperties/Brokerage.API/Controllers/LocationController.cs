using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brokerage.API.Controllers;

/// <summary>
/// Handles HTTP requests for location search data.
/// </summary>
/// <remarks>
/// Flow:
/// LocationController
///     → ILocationService
///     → IndiaLocationService
///     → CountriesNow
///
/// Location data is external reference data and does not use
/// the application's SQL repository layer.
/// </remarks>
[ApiController]
[Route("api/locations")]
public sealed class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationController(
        ILocationService locationService)
    {
        _locationService = locationService;
    }

    /// <summary>
    /// Searches Indian states and cities by name prefix.
    /// </summary>
    /// <param name="search">
    /// Optional prefix such as Che or Tami.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel the request.
    /// </param>
    /// <returns>
    /// Matching Indian states and cities.
    /// </returns>
    [Authorize(Roles = "Buyer")]
    [HttpGet("india")]
    public async Task<ActionResult<
        IReadOnlyList<LocationSearchOptionDto>>>
        SearchIndiaLocations(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
    {
        IReadOnlyList<LocationSearchOptionDto> results =
            await _locationService.SearchIndiaAsync(
                search,
                cancellationToken);

        return Ok(results);
    }
}