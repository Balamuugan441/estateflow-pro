using System.ComponentModel.DataAnnotations;

namespace Brokerage.Web.Models;

public class AddPropertyDetailsViewModel
{
    [Required(ErrorMessage = "Rent price is required.")]
    [Range(1, double.MaxValue, ErrorMessage = "Enter a valid rent price.")]
    public decimal? Price { get; set; }

    [Required(ErrorMessage = "Security deposit is required.")]
    [Range(0, double.MaxValue, ErrorMessage = "Enter a valid security deposit.")]
    public decimal? SecurityDeposit { get; set; }

    [Required(ErrorMessage = "Area is required.")]
    [Range(1, double.MaxValue, ErrorMessage = "Enter a valid area.")]
    public decimal? Area { get; set; }

    [Required(ErrorMessage = "Area unit is required.")]
    public string AreaUnit { get; set; } = string.Empty;
    [Required(ErrorMessage = "Bedrooms are required.")]

    public string Bedrooms { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bathrooms are required.")]
    public decimal? Bathrooms { get; set; }

    public int? Balconies { get; set; }

    [Required(ErrorMessage = "Floor is required.")]
    public string Floor { get; set; } = string.Empty;
    [Required(ErrorMessage = "Parking Spaces are required.")]

    public int? ParkingSpaces { get; set; }

    public int? YearBuilt { get; set; }

    [Required(ErrorMessage = "Property age is required.")]
    [Range(0, 200, ErrorMessage = "Enter a valid property age.")]
    public int? PropertyAgeYears { get; set; }

    [Required(ErrorMessage = "Possession date is required.")]
    [DataType(DataType.Date)]
    public DateTime? PossessionDate { get; set; }

    public string? FurnishingType { get; set; }

    public string? FacingDirection { get; set; }

    public string? PreferredTenants { get; set; }

    public string? TenantFoodPreference { get; set; }

    public string? Description { get; set; }
}