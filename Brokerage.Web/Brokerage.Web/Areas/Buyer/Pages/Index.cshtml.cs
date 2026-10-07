using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiRequests;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Buyer.Pages;

/// <summary>
/// Loads and serves the Buyer property listing page.
/// </summary>
/// <remarks>
/// Flow:
/// IndexModel
///     → PropertyApiClient
///     → Brokerage.API
///     → PropertyService
///     → PropertyRepository
///     → SQL Server
/// </remarks>
public sealed class IndexModel : PageModel
{
    private readonly PropertyApiClient _propertyApiClient;

    public IndexModel(PropertyApiClient propertyApiClient)
    {
        _propertyApiClient = propertyApiClient;
    }

    public IReadOnlyList<BuyerPropertyApiModel> Properties { get; private set; } = [];

    public int TotalRecords { get; private set; }

    public int CurrentPage { get; private set; } = 1;

    public int TotalPages { get; private set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < TotalPages;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        BuyerPropertySearchRequest request = new();

        BuyerPropertyListApiResponse response = await _propertyApiClient.SearchBuyerPropertiesAsync(request, cancellationToken);

        Properties = response.Properties;
        TotalRecords = response.TotalRecords;
        CurrentPage = response.PageNumber;
        TotalPages = response.TotalPages;
    }

    public async Task<IActionResult> OnGetSearchPropertiesAsync(
        [FromQuery] BuyerPropertySearchRequest request,
        CancellationToken cancellationToken)
    {
        BuyerPropertyListApiResponse response = await _propertyApiClient.SearchBuyerPropertiesAsync(
            request,
            cancellationToken);

        return new JsonResult(response);
    }

    public async Task<IActionResult> OnGetLocationSearchAsync(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        List<LocationSearchOptionApiModel> locations = await _propertyApiClient.SearchIndiaLocationsAsync(
            search,
            cancellationToken);

        return new JsonResult(locations);
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

        HttpResponseMessage response = await _propertyApiClient.GetPropertyImageAsync(
            path,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            return NotFound();
        }

        string contentType = response.Content.Headers.ContentType?.ToString()
            ?? "application/octet-stream";

        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync(
            cancellationToken);

        response.Dispose();

        return File(
            imageBytes,
            contentType);
    }
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> OnPostFavoriteAsync(
    Guid propertyGuid,
    bool favorite,
    CancellationToken cancellationToken)
    {
        try
        {
            ApiMessageResponse response;

            if (favorite)
            {
                response =
                    await _propertyApiClient
                        .AddFavoriteAsync(
                            propertyGuid,
                            cancellationToken);
            }
            else
            {
                response =
                    await _propertyApiClient
                        .RemoveFavoriteAsync(
                            propertyGuid,
                            cancellationToken);
            }

            return new JsonResult(
                new
                {
                    success = response.Success,
                    message = response.Message,
                    isFavorite = favorite
                });
        }
        catch (HttpRequestException ex)
        {
            return new JsonResult(
                new
                {
                    success = false,
                    message = "Unable to update favorite."
                })
            {
                StatusCode =
                    (int?)ex.StatusCode
                    ?? StatusCodes.Status500InternalServerError
            };
        }
    }

    // Generates the pagination sequence containing page numbers and null values for ellipses.

    public List<int?> GetPaginationSequence()
    {
        List<int?> pages = new List<int?>();

        if (TotalPages <= 0)
        {
            return pages;
        }

        if (TotalPages <= 7)
        {
            for (int i = 1; i <= TotalPages; i++)
            {
                pages.Add(i);
            }
            return pages;
        }

        int start = CurrentPage - 2;
        int end = CurrentPage + 2;

        if (start <= 3)
        {
            start = 1;
            end = 5;
        }
        else if (end >= TotalPages - 2)
        {
            start = TotalPages - 4;
            end = TotalPages;
        }

        pages.Add(1);

        if (start > 2)
        {
            pages.Add(null);
        }

        for (int i = Math.Max(2, start); i <= Math.Min(TotalPages - 1, end); i++)
        {
            pages.Add(i);
        }

        if (end < TotalPages - 1)
        {
            pages.Add(null);
        }

        pages.Add(TotalPages);

        return pages;
    }
}