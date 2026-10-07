namespace Brokerage.Web.Models.ApiResponses;

/// <summary>
/// Represents the API response returned after creating a Buyer purchase request.
/// </summary>
public class CreateBuyRequestApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? BuyRequestID { get; set; }
}