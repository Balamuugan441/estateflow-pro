using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Seller.Pages.MyProperties;


public class PropertyCardViewModel
{
    public PropertyApiModel Property { get; set; } = null!;

    public string? CoverImageUrl { get; set; }
}


public class IndexModel : PageModel
{
    private readonly PropertyApiClient _propertyApiClient;

    private readonly IConfiguration _configuration;


    public List<PropertyCardViewModel> Properties { get; private set; } = [];


    public IndexModel(
        PropertyApiClient propertyApiClient,
        IConfiguration configuration)
    {
        _propertyApiClient = propertyApiClient;

        _configuration = configuration;
    }


    /* GET MY PROPERTIES */

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            List<PropertyApiModel> properties =
                await _propertyApiClient
                    .GetMyPropertiesAsync(
                        cancellationToken);


            string apiBaseUrl =
                _configuration[
                    "ApiSettings:BaseUrl"] ?? string.Empty;


            List<PropertyCardViewModel> propertyCards = [];


            foreach (PropertyApiModel property in properties)
            {
                List<PropertyMediaApiModel> media =
  await _propertyApiClient
                  .GetPropertyMediaAsync(
          property.PropertyID,
          cancellationToken);


                PropertyMediaApiModel? coverMedia =
                    media.FirstOrDefault(
                        x =>
                            x.MediaType ==
                            "CoverPhoto");


                string? coverImageUrl = null;


                if (coverMedia != null &&
                    !string.IsNullOrWhiteSpace(
                        coverMedia.FilePath))
                {
                    coverImageUrl =
                        BuildMediaUrl(
                            apiBaseUrl,
                            coverMedia.FilePath);
                }


                propertyCards.Add(
                    new PropertyCardViewModel
                    {
                        Property = property,

                        CoverImageUrl =
                            coverImageUrl
                    });
            }


            Properties =
                propertyCards;


            return Page();
        }


        catch (HttpRequestException ex)
            when (ex.StatusCode ==
                  System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToPage(
                "/Account/Login");
        }

        catch (HttpRequestException ex)
            when (ex.StatusCode ==
                  System.Net.HttpStatusCode.Forbidden)
        {
            return RedirectToPage(
                "/Account/AccessDenied");
        }


        catch (Exception)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to load your properties.");

            return Page();
        }
    }


    private static string BuildMediaUrl(
        string apiBaseUrl,
        string filePath)
    {

        if (Uri.TryCreate(
                filePath,
                UriKind.Absolute,
                out Uri? absoluteUri))
        {
            return absoluteUri.ToString();
        }


        string baseUrl =
            apiBaseUrl.TrimEnd('/');


        string path =
            filePath.StartsWith("/")
                ? filePath
                : "/" + filePath;


        return baseUrl + path;
    }
}