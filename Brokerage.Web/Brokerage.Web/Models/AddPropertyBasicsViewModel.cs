using System.ComponentModel.DataAnnotations;

namespace Brokerage.Web.Models;

public class AddPropertyBasicsViewModel
{
    [Required(ErrorMessage = "Property title is required.")]
    [StringLength(
        100,
        ErrorMessage = "Property title cannot exceed 100 characters.")]
    public string PropertyTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Property type is required.")]
    public string PropertyType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Listing type is required.")]
    public string ListingType { get; set; } = string.Empty;
    [Required(ErrorMessage = "Property Status is required.")]

    public string PropertyStatus { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location address is required.")]
    public string LocationAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required.")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required.")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Zip code is required.")]
    [RegularExpression(@"^[0-9]{6}$", ErrorMessage = "Zip code must be exactly 6 digits.")]
    public string ZipCode { get; set; } = string.Empty;
}