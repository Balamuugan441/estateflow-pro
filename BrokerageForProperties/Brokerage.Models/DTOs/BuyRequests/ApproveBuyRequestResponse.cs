namespace Brokerage.Models.DTOs.BuyRequests;


// Represents the result of approving a Seller purchase request.

public class ApproveBuyRequestResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? TransactionID { get; set; }
}