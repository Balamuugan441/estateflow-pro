using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Properties;
using Dapper;
using System.Data;

namespace Brokerage.Data.Repositories;

// Handles database operations for Buyer property favorites.
public class FavoriteRepository : IFavoriteRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public FavoriteRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Adds an approved property to the buyer's favorites when it is not already saved.
    public async Task<bool> AddFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "dbo.usp_Favorite_Add",
                    new
                    {
                        UserID = userId,
                        PropertyGUID = propertyGuid
                    },
                    commandType: CommandType.StoredProcedure));

        return rowsAffected > 0;
    }

    // Removes the specified property from the buyer's favorites.
    public async Task<bool> RemoveFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    "dbo.usp_Favorite_Remove",
                    new
                    {
                        UserID = userId,
                        PropertyGUID = propertyGuid
                    },
                    commandType: CommandType.StoredProcedure));

        return rowsAffected > 0;
    }

    // Checks whether the specified buyer has saved the property as a favorite.
    public async Task<bool> IsFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "dbo.usp_Favorite_IsFavorite",
                new
                {
                    UserID = userId,
                    PropertyGUID = propertyGuid
                },
                commandType: CommandType.StoredProcedure));
    }

    // Retrieves the buyer's approved favorite properties with pagination and cover images.
    public async Task<BuyerFavoriteListResponse> GetFavoritesAsync(
        int userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber =
            pageNumber < 1
                ? 1
                : pageNumber;

        pageSize =
            pageSize < 1
                ? 6
                : pageSize;

        using var connection =
            _connectionFactory.CreateConnection();

        CommandDefinition command =
            new(
                "dbo.usp_Favorite_GetPaged",
                new
                {
                    UserID = userId,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

        using SqlMapper.GridReader grid =
            await connection.QueryMultipleAsync(
                command);

        int totalRecords =
            await grid.ReadSingleAsync<int>();

        IEnumerable<BuyerPropertyCardDto> properties =
            await grid.ReadAsync<BuyerPropertyCardDto>();

        int totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        return new BuyerFavoriteListResponse
        {
            Properties =
                properties.ToList(),

            PageNumber =
                pageNumber,

            PageSize =
                pageSize,

            TotalRecords =
                totalRecords,

            TotalPages =
                totalPages
        };
    }
}