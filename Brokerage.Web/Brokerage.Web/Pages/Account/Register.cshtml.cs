using Brokerage.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using Brokerage.Web.Models.ApiResponses;

namespace Brokerage.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RegisterModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }
    [BindProperty]
    public RegisterViewModel Register { get; set; } = new();
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Check validation errors from the Web form.
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Create the data that the API expects.
        var request = new
        {
            FullName = Register.FullName,
            Email = Register.Email,
            MobileNumber = Register.MobileNumber,
            Role = Register.Role,
            Password = Register.Password
        };

        // Get the API HttpClient.
        var client =
            _httpClientFactory.CreateClient("BrokerageApi");

        // Send registration data to the API.
        var response =
            await client.PostAsJsonAsync(
                "api/Auth/register",
                request);

        // Read the response from the API.
        var result =
            await response.Content.ReadFromJsonAsync<RegisterApiResponse>();

        // If API says registration failed.
        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                "Register.Email",
                result?.Message ?? "Registration failed.");

            return Page();
        }

        // Registration successful.
        return RedirectToPage("/Account/Login");
    }
}