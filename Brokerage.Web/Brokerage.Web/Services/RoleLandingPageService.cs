namespace Brokerage.Web.Services;

public sealed class RoleLandingPageService
{
    public string? GetLandingPage(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "admin" => "/Admin/Users",
            "seller" => "/Seller/MyProperties",
            "buyer" => "/Buyer",
            _ => null
        };
    }
}