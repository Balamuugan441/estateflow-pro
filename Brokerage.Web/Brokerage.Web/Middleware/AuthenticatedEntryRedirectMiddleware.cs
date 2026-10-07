using Brokerage.Web.Services;
using System.Security.Claims;

namespace Brokerage.Web.Middleware;

public class AuthenticatedEntryRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticatedEntryRedirectMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        RoleLandingPageService roleLandingPageService)
    {

        // Only authenticated users are redirected.

        //Logged-out users continue normally.

        if (context.User.Identity?.IsAuthenticated == true)
        {
            string path =
              context.Request.Path.Value ?? "/";

            bool isPublicEntryRoute =
                path.Equals(
                    "/",
                    StringComparison.OrdinalIgnoreCase)
                ||
                path.Equals(
                    "/Account/Login",
                    StringComparison.OrdinalIgnoreCase)
                ||
                path.Equals(
                    "/Account/Register",
                    StringComparison.OrdinalIgnoreCase);

            if (isPublicEntryRoute)
            {
                string? role =
                    context.User.FindFirstValue(
                        ClaimTypes.Role);

                string? landingPage =
                    roleLandingPageService.GetLandingPage(
                        role);

                if (!string.IsNullOrWhiteSpace(
                        landingPage))
                {
                    context.Response.Redirect(
                        landingPage);

                    return;
                }
            }
        }

        await _next(context);
    }
}