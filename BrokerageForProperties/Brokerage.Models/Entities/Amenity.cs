namespace Brokerage.Models.Entities;

public class Amenity
{
    public int AmenityID { get; set; }

    public string Category { get; set; } = string.Empty;

    public string AmenityName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}