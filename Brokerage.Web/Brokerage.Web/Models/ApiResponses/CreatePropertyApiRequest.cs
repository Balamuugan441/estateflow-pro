namespace Brokerage.Web.Models.ApiRequests;

public class CreatePropertyApiRequest
{
    public string PropertyTitle { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string ListingType { get; set; } = string.Empty;

    public string PropertyStatus { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public decimal? SecurityDeposit { get; set; }

    public decimal? Area { get; set; }

    public string AreaUnit { get; set; } = string.Empty;

    public string Bedrooms { get; set; } = string.Empty;

    public decimal? Bathrooms { get; set; }

    public int? Balconies { get; set; }

    public string? Floor { get; set; }

    public int? ParkingSpaces { get; set; }

    public int? YearBuilt { get; set; }

    public int? PropertyAgeYears { get; set; }

    public DateTime? PossessionDate { get; set; }

    public string? FurnishingType { get; set; }

    public string? FacingDirection { get; set; }

    public string? PreferredTenants { get; set; }

    public string? TenantFoodPreference { get; set; }

    public string? Description { get; set; }
}