using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Models.ApiResponses;

/// <summary>
/// Represents the paginated response returned by the Buyer property listing API.
/// </summary>
public sealed class BuyerPropertyListApiResponse
{
    public List<BuyerPropertyApiModel> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}