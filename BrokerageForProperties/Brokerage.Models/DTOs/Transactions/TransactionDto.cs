namespace Brokerage.Models.DTOs.Transactions;


/// Represents a completed property transaction displayed in the
/// Admin Transactions page.

public class TransactionDto
{
    public int TransactionID { get; set; }

    public int BuyRequestID { get; set; }

    public int PropertyID { get; set; }

    public Guid PropertyGUID { get; set; }

    public string PropertyTitle { get; set; } = string.Empty;

    public string? PropertyLocation { get; set; }

    public int BuyerID { get; set; }

    public string BuyerName { get; set; } = string.Empty;

    public int SellerID { get; set; }

    public string SellerName { get; set; } = string.Empty;

    public decimal TransactionAmount { get; set; }

    public string TransactionStatus { get; set; } = string.Empty;

    public DateTime CompletionDate { get; set; }

    public string? CoverImagePath { get; set; }
}