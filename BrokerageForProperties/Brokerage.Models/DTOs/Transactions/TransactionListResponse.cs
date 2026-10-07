namespace Brokerage.Models.DTOs.Transactions;


/// Represents a paginated collection of completed transactions
/// together with the pagination information required by the Admin UI.

public class TransactionListResponse
{
    public List<TransactionDto> Transactions { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}