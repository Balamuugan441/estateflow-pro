using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Admin.Pages.PendingApprovals;

public class PreviewModel : PageModel
{
    private readonly AdminApiClient _adminApiClient;
    private readonly IConfiguration _configuration;

    public PropertyApiModel Property { get; private set; } = new();

    public List<AmenityApiModel> Amenities { get; private set; } = [];

    public List<PropertyMediaApiModel> Media { get; private set; } = [];

    public string ApiBaseUrl { get; private set; } = string.Empty;

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

    public PreviewModel(
        AdminApiClient adminApiClient,
        IConfiguration configuration)
    {
        _adminApiClient = adminApiClient;
        _configuration = configuration;
    }

    public async Task<IActionResult> OnGetAsync(
        int propertyId,
        CancellationToken cancellationToken)
    {
        ApiBaseUrl =
            _configuration["ApiSettings:BaseUrl"]
            ?? string.Empty;

        PropertyReviewApiResponse? response =
            await _adminApiClient
                .GetPropertyPreviewAsync(
                    propertyId,
                    cancellationToken);

        if (response == null ||
            response.Property == null)
        {
            return NotFound();
        }

        Property = response.Property;

        Amenities = response.Amenities;

        Media = response.Media;

        return Page();
    }

    public string BuildMediaUrl(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        if (filePath.StartsWith(
            "http://",
            StringComparison.OrdinalIgnoreCase) ||
            filePath.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase))
        {
            return filePath;
        }

        return
            $"{ApiBaseUrl.TrimEnd('/')}/" +
            filePath.TrimStart('/');
    }
}