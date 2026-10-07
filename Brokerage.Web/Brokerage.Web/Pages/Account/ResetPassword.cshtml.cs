using Brokerage.Web.Models;
using Brokerage.Web.Models.ApiResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Pages.Account;

public class ResetPasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResetPasswordModel(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;



    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Token))
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid password reset link.");

            return Page();
        }

        var client =
            _httpClientFactory.CreateClient("BrokerageApi");

        var request = new
        {
            Token = Token,
            NewPassword = Input.NewPassword,
            ConfirmPassword = Input.ConfirmPassword
        };

        var response =
            await client.PostAsJsonAsync(
                "api/Auth/reset-password",
                request);

        var result =
            await response.Content
                .ReadFromJsonAsync<ResetPasswordResponse>();

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                result?.Message ??
                "Unable to reset your password.");

            return Page();
        }

        TempData["ResetPasswordMessage"] =
            result?.Message ??
            "Your password has been reset successfully.";

        return RedirectToPage("./Login");
    }
}