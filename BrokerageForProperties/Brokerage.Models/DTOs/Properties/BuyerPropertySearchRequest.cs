/// <summary>
/// Represents the filtering and pagination criteria used to search approved Buyer property listings.
/// </summary>
public sealed class BuyerPropertySearchRequest
{
    public int PageNumber { get; set; } = 1;

    public string? Search { get; set; }

    public string? LocationType { get; set; }

    public string? LocationValue { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? PropertyType { get; set; }

    public int? MinBedrooms { get; set; }

    public List<string> ListingTypes { get; set; } = [];

    public string SortBy { get; set; } = "latest";
}