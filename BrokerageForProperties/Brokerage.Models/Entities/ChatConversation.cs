namespace Brokerage.Models.Entities;


/// Represents a property-specific conversation between one Buyer and one Seller.

public class ChatConversation
{
    public int ConversationID { get; set; }

    public int PropertyID { get; set; }

    public int BuyerID { get; set; }

    public int SellerID { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastMessageAt { get; set; }
}