using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace Brokerage.Web.Areas.Buyer.Pages.Properties;

/// <summary>
/// Loads an approved property and provides access to its amenities and media
/// for the Buyer property preview page.
/// </summary>
public class PreviewModel : PageModel
{
    private readonly PropertyApiClient _propertyApiClient;

    public PropertyApiModel Property { get; private set; } = new();

    public List<AmenityApiModel> Amenities { get; private set; } = [];

    public List<PropertyMediaApiModel> Media { get; private set; } = [];
    public bool IsFavorite { get; private set; }
    public bool IsOpenedFromFavorites { get; private set; }

    public PropertyMediaApiModel? CoverPhoto =>
        Media.FirstOrDefault(
            media =>
                media.MediaType == "CoverPhoto");

    public List<PropertyMediaApiModel> GalleryImages =>
        Media
            .Where(
                media =>
                    media.MediaType == "GalleryImage")
            .OrderBy(
                media =>
                    media.DisplayOrder)
            .ThenBy(
                media =>
                    media.MediaID)
            .ToList();

    public List<PropertyMediaApiModel> Videos =>
        Media
            .Where(
                media =>
                    media.MediaType == "Video")
            .OrderBy(
                media =>
                    media.MediaID)
            .ToList();

    public List<PropertyMediaApiModel> FloorPlans =>
        Media
            .Where(
                media =>
                    media.MediaType == "FloorPlan")
            .OrderBy(
                media =>
                    media.MediaID)
            .ToList();

    public List<PropertyMediaApiModel> Documents =>
        Media
            .Where(
                media =>
                    media.MediaType == "Document")
            .OrderBy(
                media =>
                    media.MediaID)
            .ToList();
    private readonly ChatApiClient _chatApiClient;
    public PreviewModel(
    PropertyApiClient propertyApiClient,
    ChatApiClient chatApiClient)
    {
        _propertyApiClient = propertyApiClient;
        _chatApiClient = chatApiClient;
    }

    public async Task<IActionResult> OnGetAsync(
    Guid propertyGuid,
    string? source,
    CancellationToken cancellationToken)
    {
        try
        {
            PropertyReviewApiResponse? response =
                await _propertyApiClient
                    .GetPropertyReviewByGuidAsync(
                        propertyGuid,
                        cancellationToken);

            if (response == null ||
                response.Property == null)
            {
                return RedirectToPage(
                    "/Properties/Unavailable",
                    new
                    {
                        area = "Buyer",
                        source
                    });
            }

            Property = response.Property;

            Amenities = response.Amenities;

            Media = response.Media;


            FavoriteStatusApiResponse favorite =
                await _propertyApiClient
                    .IsFavoriteAsync(
                        propertyGuid,
                        cancellationToken);

            IsFavorite =
                favorite.IsFavorite;


            IsOpenedFromFavorites =
                string.Equals(
                    source,
                    "favorites",
                    StringComparison.OrdinalIgnoreCase);


            return Page();
        }
        catch (HttpRequestException ex)
            when (
                ex.StatusCode ==
                HttpStatusCode.NotFound)
        {
            return RedirectToPage(
                "/Properties/Unavailable",
                new
                {
                    area = "Buyer",
                    source
                });
        }
    }
    public string BuildMediaUrl(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        return Url.Page(
            "/Properties/Preview",
            new
            {
                area = "Buyer",
                handler = "PropertyImage",
                path = filePath
            }) ?? string.Empty;
    }

    public async Task<IActionResult> OnGetPropertyImageAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return NotFound();
        }

        string normalizedPath =
            path.Replace("\\", "/");

        if (!normalizedPath.StartsWith(
                "/uploads/properties/",
                StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath =
                "/" + normalizedPath.TrimStart('/');
        }

        if (!normalizedPath.StartsWith(
                "/uploads/properties/",
                StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        HttpResponseMessage response =
            await _propertyApiClient
                .GetPropertyImageAsync(
                    normalizedPath,
                    cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        byte[] fileBytes =
            await response.Content.ReadAsByteArrayAsync(
                cancellationToken);

        string contentType =
            response.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        return File(
            fileBytes,
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
    public async Task<IActionResult> OnPostBuyNowAsync(
    Guid propertyGuid,
    CancellationToken cancellationToken)
    {
        try
        {
            CreateBuyRequestApiResponse response =
                await _propertyApiClient
                    .CreateBuyRequestAsync(
                        propertyGuid,
                        cancellationToken);

            return new JsonResult(
                new
                {
                    success = response.Success,
                    message = response.Message,
                    buyRequestID = response.BuyRequestID
                });
        }
        catch (HttpRequestException ex)
        {
            return new JsonResult(
                new
                {
                    success = false,
                    message = "Unable to send the buy request."
                })
            {
                StatusCode =
                    (int?)ex.StatusCode
                    ?? StatusCodes.Status500InternalServerError
            };
        }
    }
    public async Task<IActionResult> OnPostStartChatAsync(
    Guid propertyGuid,
    CancellationToken cancellationToken)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return Challenge();
        }

        if (!User.IsInRole("Buyer"))
        {
            return Forbid();
        }

        try
        {
            var conversation =
                await _chatApiClient.StartConversationAsync(
                    propertyGuid,
                    cancellationToken);

            return RedirectToPage(
                "/Messages/Index",
                new
                {
                    area = "Buyer",
                    conversationId = conversation.ConversationID
                });
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to start the conversation right now.");

            return Page();
        }
    }
}