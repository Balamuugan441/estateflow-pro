using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Transactions;
using Dapper;
using System.Data;

namespace Brokerage.Data.Repositories;

// Handles database operations for retrieving completed property transactions with server-side filtering and pagination.
public class TransactionRepository : ITransactionRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TransactionRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory =
            connectionFactory;
    }

    // Retrieves completed transactions using stored-procedure filtering, sorting, and pagination.
    public async Task<TransactionListResponse> GetCompletedTransactionsAsync(
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

        if (pageSize > 50)
        {
            pageSize = 50;
        }

        using var connection =
            _connectionFactory.CreateConnection();

        CommandDefinition command =
            new(
                "dbo.usp_Transaction_GetCompletedPaged",
                new
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MinPrice = request.MinPrice,
                    MaxPrice = request.MaxPrice,
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

        IEnumerable<TransactionDto> transactions =
            await grid.ReadAsync<TransactionDto>();

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