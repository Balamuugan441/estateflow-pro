using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Transactions;
using Dapper;

namespace Brokerage.Data.Repositories;


// Handles database operations for retrieving completed property
// transactions with server-side filtering and pagination.

public class TransactionRepository : ITransactionRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TransactionRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory =
            connectionFactory;
    }
    //Gets all the completed transaction history by also performing fitering and pagination from the server side
    public async Task<TransactionListResponse>
        GetCompletedTransactionsAsync(
            TransactionFilterRequest request,
            CancellationToken cancellationToken = default)
    {
        int pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        int pageSize =
            request.PageSize < 1
                ? 4
                : request.PageSize;

        int offset =
            (pageNumber - 1) * pageSize;

        const string countQuery = """
            SELECT
                COUNT(1)
            FROM dbo.Transactions t
            WHERE t.TransactionStatus = 'Completed'
              AND
              (
                  @StartDate IS NULL
                  OR t.CompletionDate >= @StartDate
              )
              AND
              (
                  @EndDate IS NULL
                  OR t.CompletionDate < DATEADD(DAY, 1, @EndDate)
              )
              AND
              (
                  @MinPrice IS NULL
                  OR t.TransactionAmount >= @MinPrice
              )
              AND
              (
                  @MaxPrice IS NULL
                  OR t.TransactionAmount <= @MaxPrice
              );
            """;

        const string dataQuery = """
            SELECT
                t.TransactionID,
                t.BuyRequestID,
                t.PropertyID,
                t.PropertyGUID,
                t.PropertyTitle,
                t.PropertyLocation,
                t.BuyerID,
                t.BuyerName,
                t.SellerID,
                t.SellerName,
                t.TransactionAmount,
                t.TransactionStatus,
                t.CompletionDate,

                cover.FilePath AS CoverImagePath

            FROM dbo.Transactions t

            OUTER APPLY
            (
                SELECT TOP 1
                    pm.FilePath
                FROM dbo.PropertyMedia pm
                WHERE pm.PropertyID = t.PropertyID
                  AND pm.MediaType = 'CoverPhoto'
                ORDER BY
                    CASE
                        WHEN pm.DisplayOrder IS NULL
                        THEN 999
                        ELSE pm.DisplayOrder
                    END,
                    pm.MediaID
            ) cover

            WHERE t.TransactionStatus = 'Completed'
              AND
              (
                  @StartDate IS NULL
                  OR t.CompletionDate >= @StartDate
              )
              AND
              (
                  @EndDate IS NULL
                  OR t.CompletionDate < DATEADD(DAY, 1, @EndDate)
              )
              AND
              (
                  @MinPrice IS NULL
                  OR t.TransactionAmount >= @MinPrice
              )
              AND
              (
                  @MaxPrice IS NULL
                  OR t.TransactionAmount <= @MaxPrice
              )

            ORDER BY
                t.CompletionDate DESC,
                t.TransactionID DESC

            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        int totalRecords =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    countQuery,
                    new
                    {
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        MinPrice = request.MinPrice,
                        MaxPrice = request.MaxPrice
                    },
                    cancellationToken:
                        cancellationToken));

        IEnumerable<TransactionDto> transactions =
            await connection.QueryAsync<TransactionDto>(
                new CommandDefinition(
                    dataQuery,
                    new
                    {
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        MinPrice = request.MinPrice,
                        MaxPrice = request.MaxPrice,
                        Offset = offset,
                        PageSize = pageSize
                    },
                    cancellationToken:
                        cancellationToken));

        int totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        return new TransactionListResponse
        {
            Transactions =
                transactions.ToList(),

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