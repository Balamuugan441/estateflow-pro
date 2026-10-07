using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Admin.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly AdminApiClient _adminApiClient;

    public AdminDashboardApiResponse Dashboard { get; private set; } = new();

    public DateTime LastUpdated { get; private set; }

    public IndexModel(
        AdminApiClient adminApiClient)
    {
        _adminApiClient = adminApiClient;
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        Dashboard =
            await _adminApiClient
                .GetDashboardAsync(
                    cancellationToken);

        LastUpdated = DateTime.Now;
    }
}