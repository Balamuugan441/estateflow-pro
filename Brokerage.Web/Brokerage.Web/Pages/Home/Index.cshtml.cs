using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Pages.Home;

public class IndexModel : PageModel
{
    private readonly HomeApiClient _homeApiClient;
    private readonly PropertyApiClient _propertyApiClient;

    public IndexModel(HomeApiClient homeApiClient, PropertyApiClient propertyApiClient)
    {
        _homeApiClient = homeApiClient;
        _propertyApiClient = propertyApiClient;
    }

    public int TotalUsers { get; private set; }

    public int ApprovedProperties { get; private set; }

    public List<PublicPropertyApiModel> Properties { get; private set; } = [];

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        PublicHomeStatsApiModel stats =
            await _homeApiClient.GetStatsAsync(
                cancellationToken);

        TotalUsers = stats.TotalUsers;

        ApprovedProperties =
            stats.ApprovedProperties;

        Properties =
            await _homeApiClient.GetLatestPropertiesAsync(
                null,
                cancellationToken);
    }

    public async Task<IActionResult> OnGetPropertiesAsync(
     string? city,
     int? bedrooms,
     int? bathrooms,
     string? listingType,
     decimal? minPrice,
     decimal? maxPrice,
     CancellationToken cancellationToken)
    {
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

        List<PublicPropertyApiModel> properties =
            await _homeApiClient.GetLatestPropertiesAsync(
                city,
                bedrooms,
                bathrooms,
                listingType,
                minPrice,
                maxPrice,
                cancellationToken);

        return new JsonResult(properties);
    }
    public async Task<IActionResult> OnGetPropertyImageAsync(
    string path,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest();
        }

        if (!path.StartsWith(
            "/uploads/properties/",
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        HttpResponseMessage response =
            await _propertyApiClient.GetPropertyImageAsync(
                path,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            return NotFound();
        }

        string contentType =
            response.Content.Headers.ContentType?.ToString()
            ?? "application/octet-stream";

        byte[] imageBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        response.Dispose();

        return File(
            imageBytes,
            contentType);
    }
}