using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Models.ApiResponses;

/// <summary>
/// Represents a paginated collection of completed transactions returned by the API.
/// </summary>
public class TransactionListApiResponse
{
    public List<TransactionApiModel> Transactions { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}