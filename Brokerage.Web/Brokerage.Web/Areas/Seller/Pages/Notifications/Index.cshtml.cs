using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Seller.Pages.Notifications;

/// <summary>
/// Loads and manages notification history and unread notifications
/// for the authenticated Seller.
/// </summary>
public class IndexModel : PageModel
{
    private readonly NotificationApiClient _notificationApiClient;

    public List<NotificationApiModel> Notifications
    {
        get;
        private set;
    } = [];

    public IndexModel(
        NotificationApiClient notificationApiClient)
    {
        _notificationApiClient =
            notificationApiClient;
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            Notifications =
                await _notificationApiClient
                    .GetAllAsync(
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

    public async Task<IActionResult> OnGetUnreadAsync(
        CancellationToken cancellationToken)
    {
        List<NotificationApiModel> notifications =
            await _notificationApiClient
                .GetUnreadAsync(
                    cancellationToken);

        return new JsonResult(
            notifications);
    }

    public async Task<IActionResult> OnPostReadAsync(
        int notificationId,
        CancellationToken cancellationToken)
    {
        await _notificationApiClient
            .MarkAsReadAsync(
                notificationId,
                cancellationToken);

        return new JsonResult(
            new
            {
                success = true
            });
    }

    public string GetNotificationIconClass(
        string notificationType)
    {
        return notificationType switch
        {
            "BuyRequestApproved" =>
                "icon-approved",

            "PropertyApproved" =>
                "icon-approved",

            "BuyRequestRejected" =>
                "icon-rejected",

            "PropertyRejected" =>
                "icon-rejected",

            "BuyRequest" =>
                "icon-request",

            _ =>
                "icon-request"
        };
    }

    public string FormatNotificationTime(
        DateTime createdAt)
    {
        DateTime utcCreatedAt =
            DateTime.SpecifyKind(
                createdAt,
                DateTimeKind.Utc);

        TimeSpan elapsed =
            DateTime.UtcNow -
            utcCreatedAt;

        if (elapsed.TotalMinutes < 1)
        {
            return "Just now";
        }

        if (elapsed.TotalHours < 1)
        {
            return $"{(int)elapsed.TotalMinutes}m ago";
        }

        if (elapsed.TotalDays < 1)
        {
            return $"{(int)elapsed.TotalHours}h ago";
        }

        if (elapsed.TotalDays < 7)
        {
            return $"{(int)elapsed.TotalDays}d ago";
        }

        return utcCreatedAt
            .ToLocalTime()
            .ToString(
                "dd MMM yyyy, hh:mm tt");
    }
}