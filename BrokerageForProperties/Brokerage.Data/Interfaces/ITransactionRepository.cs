using Brokerage.Models.DTOs.Transactions;

namespace Brokerage.Data.Interfaces;


// Provides database operations for retrieving properties whose Transaction Status = 'Completed'
// property transactions with filtering and pagination.

public interface ITransactionRepository
{
    Task<TransactionListResponse> GetCompletedTransactionsAsync(
        TransactionFilterRequest request,
        CancellationToken cancellationToken = default);
}