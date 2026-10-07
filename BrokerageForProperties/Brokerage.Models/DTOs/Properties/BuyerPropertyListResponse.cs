namespace Brokerage.Models.DTOs.Properties;

/// <summary>
/// Represents a paginated collection of approved properties returned to a Buyer.
/// </summary>
public sealed class BuyerPropertyListResponse
{

    public IReadOnlyList<BuyerPropertyCardDto> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}