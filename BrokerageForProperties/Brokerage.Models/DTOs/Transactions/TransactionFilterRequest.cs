namespace Brokerage.Models.DTOs.Transactions;


/// Represents the server-side filters and pagination parameters
/// used to retrieve completed Admin transactions.

public class TransactionFilterRequest
{
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 4;
}