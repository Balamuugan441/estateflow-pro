namespace Brokerage.Web.Models.ApiResponses;

// Represents the result returned after a Seller approves a purchase request.

public class ApproveBuyRequestApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? TransactionID { get; set; }
}