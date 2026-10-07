using System.ComponentModel.DataAnnotations;

namespace Brokerage.Models.DTOs.Properties;

public class CreatePropertyRequest
{
    // Step 1 - Basics

    [Required(ErrorMessage = "Property title is required.")]
    [StringLength(100)]
    public string PropertyTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property type is required.")]
    public string PropertyType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Listing type is required.")]
    public string ListingType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property status is required.")]
    public string PropertyStatus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location address is required.")]
    [StringLength(255)]
    public string LocationAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required.")]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required.")]
    [StringLength(100)]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zip code is required.")]
    [StringLength(10)]
    public string ZipCode { get; set; } = string.Empty;


    // Step 2 - Details

    [Required(ErrorMessage = "Price is required.")]
    [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    public decimal? SecurityDeposit { get; set; }

    [Required(ErrorMessage = "Area is required.")]
    [Range(1, double.MaxValue, ErrorMessage = "Area must be greater than zero.")]
    public decimal Area { get; set; }

    [Required(ErrorMessage = "Area unit is required.")]
    public string AreaUnit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bedrooms is required.")]
    public string Bedrooms { get; set; } = string.Empty;

    public decimal Bathrooms { get; set; } = 2m;

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

    [StringLength(1000)]
    public string? Description { get; set; }
}