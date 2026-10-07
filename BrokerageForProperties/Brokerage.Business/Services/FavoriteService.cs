using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Properties;

namespace Brokerage.Business.Services;


// Handles Buyer property favorite operations and applies
// the business rules for saving and removing favorites.

public class FavoriteService
{
    private readonly IFavoriteRepository _favoriteRepository;

    public FavoriteService(
        IFavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    public async Task<bool> AddFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        return await _favoriteRepository
            .AddFavoriteAsync(
                userId,
                propertyGuid);
    }

    public async Task<bool> RemoveFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        return await _favoriteRepository
            .RemoveFavoriteAsync(
                userId,
                propertyGuid);
    }

    public async Task<bool> IsFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        return await _favoriteRepository
            .IsFavoriteAsync(
                userId,
                propertyGuid);
    }

    public async Task<BuyerFavoriteListResponse> GetFavoritesAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _favoriteRepository
            .GetFavoritesAsync(
                userId,
                pageNumber,
                pageSize,
                cancellationToken);
    }
}