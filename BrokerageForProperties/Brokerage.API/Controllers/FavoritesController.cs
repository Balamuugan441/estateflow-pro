using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Brokerage.API.Controllers;


// Provides authenticated Buyer endpoints for managing saved properties.

[ApiController]
[Route("api/favorites")]
[Authorize(Roles = "Buyer")]
public class FavoritesController : ControllerBase
{
    private readonly FavoriteService _favoriteService;

    public FavoritesController(
        FavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpPost("{propertyGuid:guid}")]
    public async Task<IActionResult> AddFavorite(
        Guid propertyGuid)
    {
        if (!TryGetCurrentUserId(
                out int userId))
        {
            return Unauthorized();
        }

        bool added =
            await _favoriteService
                .AddFavoriteAsync(
                    userId,
                    propertyGuid);

        if (!added)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message =
                        "Property was not found, is not approved, or is already in favorites."
                });
        }

        return Ok(
            new
            {
                success = true,
                message =
                    "Property added to favorites."
            });
    }

    [HttpDelete("{propertyGuid:guid}")]
    public async Task<IActionResult> RemoveFavorite(
        Guid propertyGuid)
    {
        if (!TryGetCurrentUserId(
                out int userId))
        {
            return Unauthorized();
        }

        bool removed =
            await _favoriteService
                .RemoveFavoriteAsync(
                    userId,
                    propertyGuid);

        if (!removed)
        {
            return NotFound(
                new
                {
                    success = false,
                    message =
                        "Favorite property was not found."
                });
        }

        return Ok(
            new
            {
                success = true,
                message =
                    "Property removed from favorites."
            });
    }

    [HttpGet("{propertyGuid:guid}/exists")]
    public async Task<IActionResult> IsFavorite(
        Guid propertyGuid)
    {
        if (!TryGetCurrentUserId(
                out int userId))
        {
            return Unauthorized();
        }

        bool isFavorite =
            await _favoriteService
                .IsFavoriteAsync(
                    userId,
                    propertyGuid);

        return Ok(
            new
            {
                success = true,
                isFavorite
            });
    }

    [HttpGet]
    public async Task<IActionResult> GetFavorites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 6,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(
                out int userId))
        {
            return Unauthorized();
        }

        pageNumber =
            pageNumber < 1
                ? 1
                : pageNumber;

        pageSize = 6;

        BuyerFavoriteListResponse response =
            await _favoriteService
                .GetFavoritesAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    cancellationToken);

        return Ok(response);
    }

    private bool TryGetCurrentUserId(
        out int userId)
    {
        string? userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.TryParse(
            userIdClaim,
            out userId);
    }
}