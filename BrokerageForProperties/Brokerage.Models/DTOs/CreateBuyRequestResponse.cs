namespace Brokerage.Models.DTOs.BuyRequests;


// Represents the result of creating a Buyer purchase request.

public class CreateBuyRequestResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? BuyRequestID { get; set; }
}