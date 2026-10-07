namespace Brokerage.Web.Models.ApiModels;

public class AmenityApiModel
{
    public int AmenityID { get; set; }

    public string Category { get; set; } = string.Empty;

    public string AmenityName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}