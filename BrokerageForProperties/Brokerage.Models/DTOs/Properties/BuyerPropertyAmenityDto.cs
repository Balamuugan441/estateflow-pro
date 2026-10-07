namespace Brokerage.Models.DTOs.Properties;


// Represents an amenity associated with a property listing.

public sealed class BuyerPropertyAmenityDto
{

    public int AmenityID { get; set; }

    public string AmenityName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}