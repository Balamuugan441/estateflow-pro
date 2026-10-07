using Brokerage.Models.Entities;

namespace Brokerage.Models.DTOs.Properties;

public class PropertyReviewResponse
{
    public Property Property { get; set; } = new();

    public IEnumerable<Amenity> Amenities { get; set; } = [];

    public IEnumerable<PropertyMedia> Media { get; set; } = [];
}