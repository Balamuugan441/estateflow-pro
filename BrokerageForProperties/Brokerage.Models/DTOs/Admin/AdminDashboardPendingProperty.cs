namespace Brokerage.Models.DTOs.Admin;

public class AdminDashboardPendingProperty
{
    public int PropertyID { get; set; }

    public string PropertyTitle { get; set; } = string.Empty;

    public string SellerName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string ListingStatus { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }
}