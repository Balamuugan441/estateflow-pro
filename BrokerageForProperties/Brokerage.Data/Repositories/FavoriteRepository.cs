using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Properties;
using Dapper;

namespace Brokerage.Data.Repositories;

/// <summary>
/// Handles database operations for Buyer property favorites.
/// </summary>
/// <remarks>
/// Uses PropertyGUID for public property lookup while preserving
/// PropertyID for the database relationship.
///</remarks>
public class FavoriteRepository : IFavoriteRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public FavoriteRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> AddFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        const string query = """
            INSERT INTO BuyerFavorites
            (
                UserID,
                PropertyID
            )
            SELECT
                @UserID,
                p.PropertyID
            FROM Properties p
            WHERE p.PropertyGUID = @PropertyGUID
              AND p.ListingStatus = 'Approved'
              AND NOT EXISTS
              (
                  SELECT 1
                  FROM BuyerFavorites bf
                  WHERE bf.UserID = @UserID
                    AND bf.PropertyID = p.PropertyID
              );
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                new
                {
                    UserID = userId,
                    PropertyGUID = propertyGuid
                });

        return rowsAffected > 0;
    }

    public async Task<bool> RemoveFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        const string query = """
            DELETE bf
            FROM BuyerFavorites bf
            INNER JOIN Properties p
                ON bf.PropertyID = p.PropertyID
            WHERE bf.UserID = @UserID
              AND p.PropertyGUID = @PropertyGUID;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                new
                {
                    UserID = userId,
                    PropertyGUID = propertyGuid
                });

        return rowsAffected > 0;
    }

    public async Task<bool> IsFavoriteAsync(
        int userId,
        Guid propertyGuid)
    {
        const string query = """
            SELECT
                CAST(
                    CASE
                        WHEN EXISTS
                        (
                            SELECT 1
                            FROM BuyerFavorites bf
                            INNER JOIN Properties p
                                ON bf.PropertyID = p.PropertyID
                            WHERE bf.UserID = @UserID
                              AND p.PropertyGUID = @PropertyGUID
                        )
                        THEN 1
                        ELSE 0
                    END
                    AS BIT
                );
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            query,
            new
            {
                UserID = userId,
                PropertyGUID = propertyGuid
            });
    }

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

        int offset =
            (pageNumber - 1) * pageSize;

        const string countQuery = """
            SELECT COUNT(1)
            FROM BuyerFavorites bf
            INNER JOIN Properties p
                ON bf.PropertyID = p.PropertyID
            WHERE bf.UserID = @UserID
              AND p.ListingStatus = 'Approved';
            """;

        const string dataQuery = """
            SELECT
                p.PropertyID,
                p.PropertyGUID,
                p.PropertyTitle,
                p.PropertyType,
                p.ListingType,
                p.LocationAddress,
                p.Country,
                p.State,
                p.City,
                p.ZipCode,
                p.Price,
                p.Area,
                p.AreaUnit,
                p.Bedrooms,
                p.Bathrooms,

                cover.FilePath AS CoverImagePath

            FROM BuyerFavorites bf

            INNER JOIN Properties p
                ON bf.PropertyID = p.PropertyID

            OUTER APPLY
            (
                SELECT TOP 1
                    pm.FilePath

                FROM PropertyMedia pm

                WHERE pm.PropertyID = p.PropertyID
                  AND pm.MediaType = 'CoverPhoto'

                ORDER BY
                    CASE
                        WHEN pm.DisplayOrder IS NULL
                        THEN 999
                        ELSE pm.DisplayOrder
                    END,
                    pm.MediaID
            ) cover

            WHERE bf.UserID = @UserID
              AND p.ListingStatus = 'Approved'

            ORDER BY
                bf.CreatedAt DESC,
                bf.FavoriteID DESC

            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        int totalRecords =
            await connection.ExecuteScalarAsync<int>(
                countQuery,
                new
                {
                    UserID = userId
                });

        IEnumerable<BuyerPropertyCardDto> properties =
            await connection.QueryAsync<BuyerPropertyCardDto>(
                new CommandDefinition(
                    dataQuery,
                    new
                    {
                        UserID = userId,
                        Offset = offset,
                        PageSize = pageSize
                    },
                    cancellationToken: cancellationToken));

        int totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        return new BuyerFavoriteListResponse
        {
            Properties = properties.ToList(),

            PageNumber = pageNumber,

            PageSize = pageSize,

            TotalRecords = totalRecords,

            TotalPages = totalPages
        };
    }
}