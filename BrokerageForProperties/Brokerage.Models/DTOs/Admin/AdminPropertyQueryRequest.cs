namespace Brokerage.Models.DTOs.Admin;

public class AdminPropertyQueryRequest
{
    public string? Search { get; set; }

    public string? ListingStatus { get; set; }

    public string? PropertyType { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? SortBy { get; set; } = "latest";

    public int PageNumber { get; set; } = 1;
}