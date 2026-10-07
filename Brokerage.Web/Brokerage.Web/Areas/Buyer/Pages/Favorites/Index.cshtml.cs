using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Buyer.Pages.Favorites;

/// <summary>
/// Loads the authenticated Buyer's saved properties and handles
/// favorite removal for the Favorites page.
/// </summary>
public class IndexModel : PageModel
{
    private readonly PropertyApiClient _propertyApiClient;

    public List<BuyerPropertyApiModel> Properties { get; private set; } = [];

    public int PageNumber { get; private set; }

    public int TotalRecords { get; private set; }

    public int TotalPages { get; private set; }

    public IndexModel(
        PropertyApiClient propertyApiClient)
    {
        _propertyApiClient = propertyApiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        int pageNumber = 1,
        CancellationToken cancellationToken = default)
    {
        BuyerPropertyListApiResponse response =
            await _propertyApiClient
                .GetFavoritesAsync(
                    pageNumber,
                    cancellationToken);

        Properties =
            response.Properties;

        PageNumber =
            response.PageNumber;

        TotalRecords =
            response.TotalRecords;

        TotalPages =
            response.TotalPages;

        return Page();
    }

    public async Task<IActionResult> OnPostFavoriteAsync(
        Guid propertyGuid,
        CancellationToken cancellationToken)
    {
        try
        {
            ApiMessageResponse response =
                await _propertyApiClient
                    .RemoveFavoriteAsync(
                        propertyGuid,
                        cancellationToken);

            return new JsonResult(
                new
                {
                    success = response.Success,
                    message = response.Message,
                    isFavorite = false
                });
        }
        catch (HttpRequestException ex)
        {
            return new JsonResult(
                new
                {
                    success = false,
                    message = "Unable to remove favorite."
                })
            {
                StatusCode =
                    (int?)ex.StatusCode
                    ?? StatusCodes.Status500InternalServerError
            };
        }
    }
}