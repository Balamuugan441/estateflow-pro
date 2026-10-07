namespace Brokerage.Web.Models.ApiResponses;

// Represents the current favorite state of a Buyer property.

public class FavoriteStatusApiResponse
{
    public bool Success { get; set; }

    public bool IsFavorite { get; set; }
}