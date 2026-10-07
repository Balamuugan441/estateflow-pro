namespace Brokerage.Web.Models.ApiRequests;


/// Represents server-side filtering and pagination criteria for transactions.

public class TransactionSearchRequest
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 4;
}