using Brokerage.Web.Models.ApiResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Brokerage.Web.Pages.Account;

public class ForgotPasswordModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ForgotPasswordModel(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public ForgotPasswordInput Input { get; set; } = new();
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
        SuccessMessage =
            TempData["ForgotPasswordMessage"] as string;
    }

    public class ForgotPasswordInput
    {
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client =
            _httpClientFactory.CreateClient("BrokerageApi");

        var request = new
        {
            Email = Input.Email
        };

        var response =
            await client.PostAsJsonAsync(
                "api/Auth/forgot-password",
                request);

        var result =
            await response.Content
                .ReadFromJsonAsync<ForgotPasswordResponse>();

        if (!response.IsSuccessStatusCode)
        {
            ModelState.AddModelError(
                string.Empty,
                "Unable to process your request. Please try again.");

            return Page();
        }

        TempData["ForgotPasswordMessage"] =
            result?.Message ??
            "If an account exists for this email address, a password reset link has been sent.";

        return RedirectToPage("./ForgotPassword");
    }
}