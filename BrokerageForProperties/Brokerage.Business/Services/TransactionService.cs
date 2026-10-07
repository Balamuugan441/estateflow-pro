using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Transactions;

namespace Brokerage.Business.Services;

/// <summary>
/// Handles business operations for retrieving completed property
/// transactions with filtering and pagination.
/// </summary>
public class TransactionService
{
    private readonly ITransactionRepository _transactionRepository;

    public TransactionService(
        ITransactionRepository transactionRepository)
    {
        _transactionRepository =
            transactionRepository;
    }

    public async Task<TransactionListResponse>
        GetCompletedTransactionsAsync(
            TransactionFilterRequest request,
            CancellationToken cancellationToken = default)
    {
        if (request.PageNumber < 1)
        {
            request.PageNumber = 1;
        }

        if (request.PageSize < 1)
        {
            request.PageSize = 4;
        }

        if (request.PageSize > 50)
        {
            request.PageSize = 50;
        }

        return await _transactionRepository
            .GetCompletedTransactionsAsync(
                request,
                cancellationToken);
    }
}