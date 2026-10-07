using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Chat;
using Brokerage.Models.Entities;

namespace Brokerage.Business.Services;

// Handles business operations for Buyer-Seller property chat.

public class ChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IPropertyRepository _propertyRepository;

    public ChatService(
        IChatRepository chatRepository,
        IPropertyRepository propertyRepository)
    {
        _chatRepository = chatRepository;
        _propertyRepository = propertyRepository;
    }

    /// <summary>
    /// Starts a conversation for a Buyer with the Seller of
    /// an approved property.
    ///
    /// If a conversation already exists, the existing conversation
    /// is returned instead of creating another one.
    /// </summary>
    public async Task<ChatConversationDto?> StartConversationAsync(
        int buyerId,
        Guid propertyGuid,
        CancellationToken cancellationToken = default)
    {
        Property? property =
            await _propertyRepository.GetPropertyByGuidAsync(
                propertyGuid);

        if (property == null)
        {
            return null;
        }

        // Buyer can start a new conversation only for
        // an approved property.
        if (!string.Equals(
                property.ListingStatus,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // The SellerID comes from the property stored in the database.
        // We never trust the client to provide the seller ID.
        int sellerId = property.SellerID;

        return await _chatRepository
            .GetOrCreateConversationAsync(
                property.PropertyID,
                buyerId,
                sellerId,
                buyerId,
                cancellationToken);
    }


    /// <summary>
    /// Gets a conversation after verifying that the current user
    /// is a participant in that conversation.
    /// </summary>
    public async Task<ChatConversationDto?> GetConversationAsync(
        int conversationId,
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        ChatConversationDto? conversation =
            await _chatRepository
                .GetConversationAsync(
                    conversationId,
                    userId,
                    cancellationToken);

        if (conversation == null)
        {
            return null;
        }

        if (!UserCanAccessConversation(
                conversation,
                userId,
                role))
        {
            return null;
        }

        return conversation;
    }


    /// <summary>
    /// Gets all conversations belonging to a Buyer.
    /// </summary>
    public async Task<List<ChatConversationDto>>
        GetBuyerConversationsAsync(
            int buyerId,
            CancellationToken cancellationToken = default)
    {
        return await _chatRepository
            .GetBuyerConversationsAsync(
                buyerId,
                cancellationToken);
    }


    /// <summary>
    /// Gets all conversations belonging to a Seller.
    /// </summary>
    public async Task<List<ChatConversationDto>>
        GetSellerConversationsAsync(
            int sellerId,
            CancellationToken cancellationToken = default)
    {
        return await _chatRepository
            .GetSellerConversationsAsync(
                sellerId,
                cancellationToken);
    }


    /// <summary>
    /// Gets all messages from a conversation after verifying
    /// that the current user belongs to the conversation.
    /// </summary>
    public async Task<List<ChatMessageDto>> GetMessagesAsync(
        int conversationId,
        int userId,
        string role,
        CancellationToken cancellationToken = default)
    {
        ChatConversationDto? conversation =
            await GetConversationAsync(
                conversationId,
                userId,
                role,
                cancellationToken);

        if (conversation == null)
        {
            return [];
        }

        return await _chatRepository
            .GetMessagesAsync(
                conversationId,
                cancellationToken);
    }


    /// <summary>
    /// Sends a message within an existing conversation.
    /// </summary>
    public async Task<ChatMessageDto> SendMessageAsync(
        int userId,
        string role,
        SendChatMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        string messageText =
            request.MessageText?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new ArgumentException(
                "Message cannot be empty.");
        }

        if (messageText.Length > 2000)
        {
            throw new ArgumentException(
                "Message cannot contain more than 2000 characters.");
        }

        ChatConversationDto? conversation =
            await GetConversationAsync(
                request.ConversationID,
                userId,
                role,
                cancellationToken);

        if (conversation == null)
        {
            throw new InvalidOperationException(
                "The conversation was not found or you are not a participant.");
        }

        // A conversation becomes read-only after the property
        // is sold or the transaction is completed.
        if (conversation.IsReadOnly)
        {
            throw new InvalidOperationException(
                "This conversation is read-only because the property has been sold or the transaction has been completed.");
        }

        int receiverId;

        if (string.Equals(
                role,
                "Buyer",
                StringComparison.OrdinalIgnoreCase))
        {
            // A Buyer sends to the Seller.
            if (conversation.BuyerID != userId)
            {
                throw new InvalidOperationException(
                    "You are not the Buyer in this conversation.");
            }

            receiverId = conversation.SellerID;
        }
        else if (string.Equals(
                     role,
                     "Seller",
                     StringComparison.OrdinalIgnoreCase))
        {
            // A Seller sends to the Buyer.
            if (conversation.SellerID != userId)
            {
                throw new InvalidOperationException(
                    "You are not the Seller in this conversation.");
            }

            receiverId = conversation.BuyerID;
        }
        else
        {
            throw new InvalidOperationException(
                "Only Buyers and Sellers can use chat.");
        }

        ChatMessage message = new ChatMessage
        {
            ConversationID = request.ConversationID,
            SenderID = userId,
            ReceiverID = receiverId,
            MessageText = messageText
        };

        ChatMessageDto? savedMessage =
            await _chatRepository
                .CreateMessageAsync(
                    message,
                    cancellationToken);

        if (savedMessage == null)
        {
            throw new InvalidOperationException(
                "The message could not be saved.");
        }

        return savedMessage;
    }



    /// Marks messages received by the current user as read.

    public async Task<bool> MarkMessagesAsReadAsync(
     int conversationId,
     int userId,
     string role,
     CancellationToken cancellationToken = default)
    {
        var conversation =
            await _chatRepository.GetConversationAsync(
                conversationId,
                userId,
                cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException(
                "Conversation not found.");
        }

        if (!UserCanAccessConversation(
                conversation,
                userId,
                role))
        {
            throw new InvalidOperationException(
                "Conversation not found.");
        }

        return await _chatRepository.MarkMessagesAsReadAsync(
            conversationId,
            userId,
            cancellationToken);
    }



    /// Checks whether the supplied user is actually a participant
    /// in the conversation.

    private static bool UserCanAccessConversation(
        ChatConversationDto conversation,
        int userId,
        string role)
    {
        if (string.Equals(
                role,
                "Buyer",
                StringComparison.OrdinalIgnoreCase))
        {
            return conversation.BuyerID == userId;
        }

        if (string.Equals(
                role,
                "Seller",
                StringComparison.OrdinalIgnoreCase))
        {
            return conversation.SellerID == userId;
        }

        return false;
    }
}