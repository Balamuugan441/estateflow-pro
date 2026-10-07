namespace Brokerage.Models.DTOs.Admin;

public class AdminPropertyResponse
{
    public int PropertyID { get; set; }

    public Guid PropertyGUID { get; set; }

    public int SellerID { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public string PropertyTitle { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string ListingType { get; set; } = string.Empty;

    public string ListingStatus { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CoverImagePath { get; set; }
}