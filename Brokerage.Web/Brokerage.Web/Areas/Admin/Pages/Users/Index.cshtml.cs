using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly AdminApiClient _adminApiClient;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RoleName { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<AdminUserApiModel> Users { get; set; } = [];

    public int PageSize { get; private set; }

    public int TotalRecords { get; private set; }

    public int TotalPages { get; private set; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public IndexModel(AdminApiClient adminApiClient)
    {
        _adminApiClient = adminApiClient;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        AdminUserListApiResponse response = await _adminApiClient.GetUsersAsync(
            Search,
            RoleName,
            IsActive,
            FromDate,
            ToDate,
            PageNumber,
            cancellationToken);

        Users = response.Users;
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
    // Processes the status change requested from the expandable user action card.
    public async Task<IActionResult> OnPostUpdateStatusAsync(int userId, bool isActive, CancellationToken cancellationToken)
    {
        try
        {
            AdminUserStatusApiResponse response =
                await _adminApiClient.UpdateUserStatusAsync(userId, isActive, cancellationToken);

            return new JsonResult(response)
            {
                StatusCode = response.Success ? StatusCodes.Status200OK : StatusCodes.Status404NotFound

            };
        }
        catch (HttpRequestException ex)
        {
            return new JsonResult(
                new
                {
                    success = false,
                    message = ex.Message
                })
            {
                StatusCode = (int?)ex.StatusCode ?? StatusCodes.Status500InternalServerError

            };
        }
    }
}