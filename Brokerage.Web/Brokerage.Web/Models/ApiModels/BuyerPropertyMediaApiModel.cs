namespace Brokerage.Web.Models.ApiModels;

/// <summary>
/// Represents property media returned with a Buyer property listing.
/// </summary>
public sealed class BuyerPropertyMediaApiModel
{
    public int MediaID { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int? DisplayOrder { get; set; }
}