using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Brokerage.Web.Areas.Admin.Pages.PendingApprovals;

public class IndexModel : PageModel
{
    private readonly AdminApiClient _adminApiClient;
    private readonly IConfiguration _configuration;

    public List<AdminPendingApprovalApiModel> Properties { get; set; } = [];

    public int PageNumber { get; private set; }

    public int PageSize { get; private set; }

    public int TotalRecords { get; private set; }

    public int TotalPages { get; private set; }

    public string ApiBaseUrl { get; private set; } = string.Empty;

    public IndexModel(
        AdminApiClient adminApiClient,
        IConfiguration configuration)
    {
        _adminApiClient = adminApiClient;
        _configuration = configuration;
    }

    public int TotalPending =>
        TotalRecords;

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        ApiBaseUrl =
            _configuration["ApiSettings:BaseUrl"]
            ?? string.Empty;

        AdminPendingApprovalListApiResponse response =
            await _adminApiClient
                .GetPendingApprovalsAsync(
                    1,
                    cancellationToken);

        SetResponse(response);
    }

    public async Task<IActionResult>
        OnGetLoadMoreAsync(
            int pageNumber,
            CancellationToken cancellationToken)
    {
        if (pageNumber < 2)
        {
            return BadRequest();
        }

        ApiBaseUrl =
            _configuration["ApiSettings:BaseUrl"]
            ?? string.Empty;

        AdminPendingApprovalListApiResponse response =
            await _adminApiClient
                .GetPendingApprovalsAsync(
                    pageNumber,
                    cancellationToken);

        ViewData["ApiBaseUrl"] =
            ApiBaseUrl;

        return new PartialViewResult
        {
            ViewName = "_PendingApprovalCards",

            ViewData =
                new ViewDataDictionary<
                    IEnumerable<AdminPendingApprovalApiModel>>(
                    ViewData,
                    response.Properties)
        };
    }

    public async Task<IActionResult>
        OnPostApproveAsync(
            int propertyId,
            CancellationToken cancellationToken)
    {
        AdminPropertyActionApiResponse response =
            await _adminApiClient
                .UpdatePendingPropertyStatusAsync(
                    propertyId,
                    "Approved",
                    cancellationToken);

        if (!response.Success)
        {
            TempData["ErrorMessage"] =
                response.Message;
        }
        else
        {
            TempData["SuccessMessage"] =
                response.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult>
        OnPostRejectAsync(
            int propertyId,
            CancellationToken cancellationToken)
    {
        AdminPropertyActionApiResponse response =
            await _adminApiClient
                .UpdatePendingPropertyStatusAsync(
                    propertyId,
                    "Rejected",
                    cancellationToken);

        if (!response.Success)
        {
            TempData["ErrorMessage"] =
                response.Message;
        }
        else
        {
            TempData["SuccessMessage"] =
                response.Message;
        }

        return RedirectToPage();
    }

    private void SetResponse(
        AdminPendingApprovalListApiResponse response)
    {
        Properties = response.Properties;

        PageNumber = response.PageNumber;

        PageSize = response.PageSize;

        TotalRecords = response.TotalRecords;

        TotalPages = response.TotalPages;
    }
}