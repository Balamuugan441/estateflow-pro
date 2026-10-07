namespace Brokerage.Web.Models.ApiModels;

public class AdminPendingApprovalApiModel
{
    public int PropertyID { get; set; }

    public Guid PropertyGUID { get; set; }

    public int SellerID { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public string PropertyTitle { get; set; } = string.Empty;

    public string PropertyType { get; set; } = string.Empty;

    public string ListingType { get; set; } = string.Empty;

    public string LocationAddress { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime SubmittedAt { get; set; }

    public string? CoverImagePath { get; set; }
}