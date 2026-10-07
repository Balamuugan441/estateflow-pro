using Brokerage.Web.Models;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RoleLandingPageService _roleLandingPageService;
    public LoginModel(IHttpClientFactory httpClientFactory, RoleLandingPageService roleLandingPageService)
    {
        _roleLandingPageService = roleLandingPageService;
        _httpClientFactory = httpClientFactory;
    }
    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var request = new
        {
            Email = Input.Email,
            Password = Input.Password
        };
        var client = _httpClientFactory.CreateClient("BrokerageApi");
        var response = await client.PostAsJsonAsync("api/Auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
        // 6. Login failed.
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                result?.Message ?? "Invalid email or password.");

            return Page();
        }
        Response.Cookies.Append(
    "EstateFlow.Auth",
    result!.Token,
    new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddMinutes(60)
    });

        string? landingPage =
      _roleLandingPageService.GetLandingPage(
          result.Role);

        if (!string.IsNullOrWhiteSpace(landingPage))
        {
            return Redirect(landingPage);
        }

        return RedirectToPage(
            "/Account/AccessDenied");

    }
}