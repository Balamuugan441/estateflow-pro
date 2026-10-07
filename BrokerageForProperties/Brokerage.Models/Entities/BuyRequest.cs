namespace Brokerage.Models.Entities;

// Represents a Buyer's request to purchase a Seller's property.

public class BuyRequest
{
    public int BuyRequestID { get; set; }

    public int PropertyID { get; set; }

    public int BuyerID { get; set; }

    public int SellerID { get; set; }

    public string RequestStatus { get; set; } = "Pending";

    public DateTime RequestedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}