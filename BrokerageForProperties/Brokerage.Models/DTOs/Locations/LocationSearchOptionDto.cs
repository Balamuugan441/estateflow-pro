namespace Brokerage.Models.DTOs.Locations;

/// <summary>
/// Represents a searchable Indian state or city.
/// </summary>
public sealed class LocationSearchOptionDto
{

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Country { get; set; } = "India";
}