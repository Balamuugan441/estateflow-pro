using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brokerage.API.Controllers;

[ApiController]
[Route("api/Home")]
[AllowAnonymous]
public class HomeController : ControllerBase
{
    private readonly HomeService _homeService;

    public HomeController(HomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        PublicHomeStatsResponse response =
            await _homeService.GetStatsAsync();

        return Ok(response);
    }

    [HttpGet("properties")]
    public async Task<IActionResult> GetLatestProperties(
        [FromQuery] string? city)
    {
        IEnumerable<PublicPropertyResponse> properties =
            await _homeService.GetLatestPropertiesAsync(city);

        return Ok(properties);
    }
}