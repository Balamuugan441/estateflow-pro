using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Admin.Pages.Properties;

public class IndexModel : PageModel
{
    private readonly AdminApiClient _adminApiClient;
    private readonly IConfiguration _configuration;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ListingStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PropertyType { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxPrice { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<AdminPropertyApiModel> Properties { get; set; } = [];

    public int PageSize { get; private set; }

    public int TotalRecords { get; private set; }

    public int TotalPages { get; private set; }

    public int TotalLive { get; private set; }

    public int TotalPending { get; private set; }

    public int TotalRejected { get; private set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public string ApiBaseUrl { get; private set; } = string.Empty;

    public IndexModel(
        AdminApiClient adminApiClient,
        IConfiguration configuration)
    {
        _adminApiClient = adminApiClient;
        _configuration = configuration;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        ApiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? string.Empty;

        AdminPropertyListApiResponse response = await _adminApiClient.GetPropertiesAsync(
            Search,
            ListingStatus,
            PropertyType,
            MinPrice,
            MaxPrice,
            PageNumber,
            cancellationToken);

        Properties = response.Properties;
        PageNumber = response.PageNumber;
        PageSize = response.PageSize;
        TotalRecords = response.TotalRecords;
        TotalPages = response.TotalPages;
        TotalLive = response.TotalLive;
        TotalPending = response.TotalPending;
        TotalRejected = response.TotalRejected;
    }

    /// <summary>
    /// Generates the pagination sequence containing page numbers and null values for ellipses.
    /// </summary>
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

        int start = PageNumber - 2;
        int end = PageNumber + 2;

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