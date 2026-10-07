namespace Brokerage.Models.DTOs.BuyRequests;

/// <summary>
/// Contains the property identifier required to create a Buyer purchase request.
/// </summary>
public class CreateBuyRequest
{
    public Guid PropertyGUID { get; set; }
}

// Represents the purchase-request information displayed to a Seller.

