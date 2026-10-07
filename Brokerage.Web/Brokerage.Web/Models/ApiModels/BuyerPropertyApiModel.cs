namespace Brokerage.Web.Models.ApiModels;

/// <summary>
/// Represents the property information returned by the Buyer property listing API.
/// </summary>
public sealed class BuyerPropertyApiModel
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

    public List<BuyerPropertyAmenityApiModel> Amenities { get; set; } = [];

    public List<BuyerPropertyMediaApiModel> Media { get; set; } = [];
    public bool IsFavorite { get; set; }
}