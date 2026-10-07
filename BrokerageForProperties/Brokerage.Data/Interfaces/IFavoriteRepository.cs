using Brokerage.Models.DTOs.Properties;

namespace Brokerage.Data.Interfaces;


// Provides database operations for Buyer property favorites.

public interface IFavoriteRepository
{
    Task<bool> AddFavoriteAsync(
        int userId,
        Guid propertyGuid);

    Task<bool> RemoveFavoriteAsync(
        int userId,
        Guid propertyGuid);

    Task<bool> IsFavoriteAsync(
        int userId,
        Guid propertyGuid);

    Task<BuyerFavoriteListResponse> GetFavoritesAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}