using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brokerage.API.Controllers;

[ApiController]
[Route("api/Home")]
[AllowAnonymous]
public class HomeController : ControllerBase
{
    private readonly HomeService _homeService;

    public HomeController(HomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        PublicHomeStatsResponse response =
            await _homeService.GetStatsAsync();

        return Ok(response);
    }

    [HttpGet("properties")]
    public async Task<IActionResult> GetLatestProperties(
    [FromQuery] string? city,
    [FromQuery] int? bedrooms,
    [FromQuery] int? bathrooms,
    [FromQuery] string? listingType,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice)
    {
        if (bedrooms.HasValue &&
            bedrooms.Value != 1 &&
            bedrooms.Value != 2 &&
            bedrooms.Value != 3 &&
            bedrooms.Value != 4)
        {
            return BadRequest(new
            {
                message = "Select a valid bedroom count."
            });
        }

        if (bathrooms.HasValue &&
            bathrooms.Value != 1 &&
            bathrooms.Value != 2 &&
            bathrooms.Value != 3 &&
            bathrooms.Value != 4)
        {
            return BadRequest(new
            {
                message = "Select a valid bathroom count."
            });
        }

        if (!string.IsNullOrWhiteSpace(listingType) &&
            !listingType.Equals(
                "For Sale",
                StringComparison.OrdinalIgnoreCase) &&
            !listingType.Equals(
                "For Rent",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Select a valid listing type."
            });
        }

        if (minPrice.HasValue && minPrice.Value < 0 ||
            maxPrice.HasValue && maxPrice.Value < 0)
        {
            return BadRequest(new
            {
                message = "Price cannot be negative."
            });
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            return BadRequest(new
            {
                message = "Minimum price cannot exceed maximum price."
            });
        }

        string? normalizedListingType = null;

        if (!string.IsNullOrWhiteSpace(listingType))
        {
            normalizedListingType =
                listingType.Equals(
                    "For Sale",
                    StringComparison.OrdinalIgnoreCase)
                    ? "For Sale"
                    : "For Rent";
        }

        IEnumerable<PublicPropertyResponse> properties =
            await _homeService.GetLatestPropertiesAsync(
                city,
                bedrooms,
                bathrooms,
                normalizedListingType,
                minPrice,
                maxPrice);

        return Ok(properties);
    }
}