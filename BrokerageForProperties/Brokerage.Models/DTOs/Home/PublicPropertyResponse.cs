namespace Brokerage.Models.DTOs.Home;

public class PublicPropertyResponse
{
    public int PropertyID { get; set; }

    public string PropertyTitle { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string ListingType { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Bedrooms { get; set; } = string.Empty;

    public decimal Bathrooms { get; set; }

    public decimal Area { get; set; }

    public string AreaUnit { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; }
}