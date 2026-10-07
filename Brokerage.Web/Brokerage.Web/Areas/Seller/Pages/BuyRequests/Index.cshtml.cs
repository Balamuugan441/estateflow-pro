using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Seller.Pages.BuyRequests;
// Displays purchase requests received by the authenticated Seller
// and handles request rejection.
public class IndexModel : PageModel
{
    private readonly PropertyApiClient _propertyApiClient;

    public List<SellerBuyRequestApiModel> Requests
    {
        get;
        private set;
    } = [];

    public IndexModel(
        PropertyApiClient propertyApiClient)
    {
        _propertyApiClient =
            propertyApiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Requests =
                await _propertyApiClient
                    .GetSellerBuyRequestsAsync(
                        cancellationToken);

            return Page();
        }
        catch (HttpRequestException ex)
            when (
                ex.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToPage(
                "/Account/Login");
        }
        catch (HttpRequestException ex)
            when (
                ex.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
        {
            return RedirectToPage(
                "/Account/AccessDenied");
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(
        int buyRequestId,
        CancellationToken cancellationToken)
    {
        try
        {
            ApiMessageResponse response =
                await _propertyApiClient
                    .RejectBuyRequestAsync(
                        buyRequestId,
                        cancellationToken);

            if (!response.Success)
            {
                TempData["BuyRequestError"] =
                    response.Message;

                return RedirectToPage();
            }

            TempData["BuyRequestMessage"] =
                "Buy request rejected.";

            return RedirectToPage();
        }
        catch (HttpRequestException)
        {
            TempData["BuyRequestError"] =
                "Unable to reject the buy request.";

            return RedirectToPage();
        }
    }

    public string BuildImageUrl(
        string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return Url.Page(
            "/BuyRequests/Index",
            new
            {
                area = "Seller",
                handler = "PropertyImage",
                path
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
            response.Dispose();
            return NotFound();
        }

        byte[] bytes =
            await response.Content
                .ReadAsByteArrayAsync(
                    cancellationToken);

        string contentType =
            response.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        response.Dispose();

        return File(
            bytes,
            contentType);
    }
    public async Task<IActionResult> OnPostApproveAsync(
    int buyRequestId,
    CancellationToken cancellationToken)
    {
        try
        {
            ApproveBuyRequestApiResponse response =
                await _propertyApiClient
                    .ApproveBuyRequestAsync(
                        buyRequestId,
                        cancellationToken);

            if (!response.Success)
            {
                TempData["BuyRequestError"] =
                    response.Message;

                return RedirectToPage();
            }

            TempData["BuyRequestMessage"] =
                $"Deal approved successfully. Transaction ID: {response.TransactionID}";

            return RedirectToPage();
        }
        catch (HttpRequestException)
        {
            TempData["BuyRequestError"] =
                "Unable to approve the buy request.";

            return RedirectToPage();
        }
    }
}