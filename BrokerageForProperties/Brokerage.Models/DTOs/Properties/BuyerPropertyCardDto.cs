namespace Brokerage.Models.DTOs.Properties;

/// <summary>
/// Represents the property information required by the Buyer property listing.
/// </summary>
public sealed class BuyerPropertyCardDto
{

    public int PropertyID { get; set; }
    public Guid PropertyGUID { get; set; }

    public string PropertyTitle { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string ListingType { get; set; } = string.Empty;

    public string? LocationAddress { get; set; }

    public string? Country { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public string? ZipCode { get; set; }

    public decimal Price { get; set; }

    public decimal Area { get; set; }

    public string? AreaUnit { get; set; }

    public string Bedrooms { get; set; } = string.Empty;

    public decimal Bathrooms { get; set; }

    public string? CoverImagePath { get; set; }

    public IReadOnlyList<BuyerPropertyAmenityDto> Amenities { get; set; } = [];

    public IReadOnlyList<BuyerPropertyMediaDto> Media { get; set; } = [];
    public bool IsFavorite { get; set; }
}