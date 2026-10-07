namespace Brokerage.Models.DTOs.Properties;

/// <summary>
/// Represents image media that can be displayed for a Buyer property listing.
/// </summary>
public sealed class BuyerPropertyMediaDto
{

    public int MediaID { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int? DisplayOrder { get; set; }
}