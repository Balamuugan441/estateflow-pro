using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Pages.Account;

public class LogoutModel : PageModel
{
    public IActionResult OnPost()
    {
        Response.Cookies.Delete(
            "EstateFlow.Auth",
            new CookieOptions
            {
                Path = "/"
            });

        HttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity());

        return RedirectToPage("/Account/Login");
    }
}