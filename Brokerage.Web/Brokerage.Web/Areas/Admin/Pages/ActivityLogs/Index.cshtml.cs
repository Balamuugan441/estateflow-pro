using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Admin.Pages.ActivityLogs;

public class IndexModel : PageModel
{
    private readonly AdminApiClient _adminApiClient;

    public IndexModel(AdminApiClient adminApiClient)
    {
        _adminApiClient = adminApiClient;
    }

    public List<AdminActivityLogApiModel> Logs { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActorSearch { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; private set; }

    public int TotalRecords { get; private set; }

    public int TotalPages { get; private set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        AdminActivityLogQueryApiRequest request = new AdminActivityLogQueryApiRequest
        {
            FromDate = FromDate,
            ToDate = ToDate,
            ActorSearch = ActorSearch,
            ActionCategory = ActionCategory,
            PageNumber = PageNumber
        };

        AdminActivityLogListApiResponse response = await _adminApiClient.GetActivityLogsAsync(
            request,
            cancellationToken);

        Logs = response.Logs;
        PageNumber = response.PageNumber;
        PageSize = response.PageSize;
        TotalRecords = response.TotalRecords;
        TotalPages = response.TotalPages;
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