namespace Brokerage.Web.Models.ApiModels;

/// <summary>
/// Represents an amenity returned with a Buyer property listing.
/// </summary>
public sealed class BuyerPropertyAmenityApiModel
{
    public int AmenityID { get; set; }

    public string AmenityName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}