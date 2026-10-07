using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiRequests;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Admin.Pages.Transactions;

/// <summary>
/// Loads and filters completed property transactions for the Admin transaction page.
/// </summary>
public class IndexModel : PageModel
{
    private readonly TransactionApiClient _transactionApiClient;
    private readonly PropertyApiClient _propertyApiClient;

    public List<TransactionApiModel> Transactions
    {
        get;
        private set;
    } = [];

    public int PageNumber { get; private set; }

    public int PageSize { get; private set; }

    public int TotalRecords { get; private set; }

    public int TotalPages { get; private set; }

    public DateTime? FromDate { get; private set; }

    public DateTime? ToDate { get; private set; }

    public decimal? MinPrice { get; private set; }

    public decimal? MaxPrice { get; private set; }

    public IndexModel(
        TransactionApiClient transactionApiClient,
        PropertyApiClient propertyApiClient)
    {
        _transactionApiClient =
            transactionApiClient;

        _propertyApiClient =
            propertyApiClient;
    }

    public async Task<IActionResult> OnGetAsync(
     DateTime? fromDate,
     DateTime? toDate,
     decimal? minPrice,
     decimal? maxPrice,
     int pageNumber = 1,
     CancellationToken cancellationToken = default)
    {
        TransactionSearchRequest request =
            new()
            {
                FromDate = fromDate,
                ToDate = toDate,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                PageNumber = pageNumber,
                PageSize = 4
            };

        TransactionListApiResponse response =
            await _transactionApiClient
                .GetTransactionsAsync(
                    request,
                    cancellationToken);

        Transactions =
            response.Transactions;

        PageNumber =
            response.PageNumber;

        PageSize =
            response.PageSize;

        TotalRecords =
            response.TotalRecords;

        TotalPages =
            response.TotalPages;

        FromDate =
            fromDate;

        ToDate =
            toDate;

        MinPrice =
            minPrice;

        MaxPrice =
            maxPrice;

        return Page();
    }
    public string BuildPropertyImageUrl(
        string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return string.Empty;
        }

        return Url.Page(
            "/Transactions/Index",
            new
            {
                area = "Admin",
                handler = "PropertyImage",
                path = filePath
            }) ?? string.Empty;
    }

    public async Task<IActionResult>
        OnGetPropertyImageAsync(
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
}