namespace Brokerage.Web.Models.ApiModels;


// Represents a Buyer purchase request returned for the authenticated Seller.

public class SellerBuyRequestApiModel
{
    public int BuyRequestID { get; set; }

    public int PropertyID { get; set; }

    public Guid PropertyGUID { get; set; }

    public string PropertyTitle { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string ListingType { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal Area { get; set; }

    public string AreaUnit { get; set; } = string.Empty;

    public string Bedrooms { get; set; } = string.Empty;

    public decimal Bathrooms { get; set; }

    public string? CoverImagePath { get; set; }

    public int BuyerID { get; set; }

    public string BuyerName { get; set; } = string.Empty;

    public string BuyerEmail { get; set; } = string.Empty;

    public string BuyerMobileNumber { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }

    public string RequestStatus { get; set; } = string.Empty;
}