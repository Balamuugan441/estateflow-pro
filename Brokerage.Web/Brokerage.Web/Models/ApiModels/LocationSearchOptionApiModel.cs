namespace Brokerage.Web.Models.ApiModels;

/// <summary>
/// Represents a searchable Indian state or city returned by the location API.
/// </summary>
public sealed class LocationSearchOptionApiModel
{
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Country { get; set; } = "India";
}