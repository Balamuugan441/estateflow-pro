using Brokerage.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Brokerage.Web.Models.ApiResponses;

namespace Brokerage.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public LoginModel(IHttpClientFactory httpClientFactory)
    {
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
        var client =  _httpClientFactory.CreateClient("BrokerageApi");
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

        // 7. Login successful.
        return RedirectToPage("/Home");
    }
}