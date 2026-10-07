namespace Brokerage.Models.DTOs.Properties;

/// <summary>
/// Represents a paginated collection of properties saved by a Buyer.
/// </summary>
public class BuyerFavoriteListResponse
{
    public List<BuyerPropertyCardDto> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}