using Brokerage.Models.DTOs.Chat;
using Brokerage.Models.Entities;

namespace Brokerage.Data.Interfaces;

/// <summary>
/// Provides database operations for property-specific Buyer and Seller chat conversations.
/// </summary>
public interface IChatRepository
{
    Task<ChatConversationDto?> GetConversationAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<ChatConversationDto?> GetOrCreateConversationAsync(
        int propertyId,
        int buyerId,
        int sellerId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<List<ChatConversationDto>> GetBuyerConversationsAsync(
        int buyerId,
        CancellationToken cancellationToken = default);

    Task<List<ChatConversationDto>> GetSellerConversationsAsync(
        int sellerId,
        CancellationToken cancellationToken = default);

    Task<List<ChatMessageDto>> GetMessagesAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task<ChatMessageDto?> CreateMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default);

    Task<bool> MarkMessagesAsReadAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default);
}