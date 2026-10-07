using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Buyer.Pages.Properties;

/// <summary>
/// Displays a friendly message when a Buyer attempts to access
/// a property that is no longer available.
/// </summary>
public class UnavailableModel : PageModel
{
    public string? Source { get; private set; }

    public void OnGet(string? source)
    {
        Source = source;
    }
}