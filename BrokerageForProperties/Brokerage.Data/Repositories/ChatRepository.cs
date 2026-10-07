using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Chat;
using Brokerage.Models.Entities;
using Dapper;

namespace Brokerage.Data.Repositories;

/// <summary>
/// Handles database operations for property-specific Buyer and Seller chat conversations.
/// </summary>
public class ChatRepository : IChatRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ChatRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ChatConversationDto?> GetConversationAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                c.ConversationID,
                c.PropertyID,

                p.PropertyGUID,
                p.PropertyTitle,

                c.BuyerID,
                buyer.FullName AS BuyerName,

                c.SellerID,
                seller.FullName AS SellerName,

                lastMessage.MessageText AS LastMessage,
                c.LastMessageAt,

                (
                    SELECT COUNT(*)
                    FROM dbo.ChatMessages cm
                    WHERE cm.ConversationID = c.ConversationID
                      AND cm.ReceiverID = @UserID
                      AND cm.IsRead = 0
                ) AS UnreadCount,

                CAST
                (
                    CASE
                        WHEN p.ListingStatus = 'Sold'
                             OR EXISTS
                             (
                                 SELECT 1
                                 FROM dbo.Transactions t
                                 WHERE t.PropertyID = c.PropertyID
                                   AND t.TransactionStatus = 'Completed'
                             )
                        THEN 1
                        ELSE 0
                    END
                    AS BIT
                ) AS IsReadOnly

            FROM dbo.ChatConversations c

            INNER JOIN dbo.Properties p
                ON c.PropertyID = p.PropertyID

            INNER JOIN dbo.Users buyer
                ON c.BuyerID = buyer.UserID

            INNER JOIN dbo.Users seller
                ON c.SellerID = seller.UserID

            OUTER APPLY
            (
                SELECT TOP 1
                    cm.MessageText
                FROM dbo.ChatMessages cm
                WHERE cm.ConversationID = c.ConversationID
                ORDER BY
                    cm.SentAt DESC,
                    cm.MessageID DESC
            ) lastMessage

            WHERE c.ConversationID = @ConversationID;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection
            .QuerySingleOrDefaultAsync<ChatConversationDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        ConversationID = conversationId,
                        UserID = userId
                    },
                    cancellationToken:
                        cancellationToken));
    }

    public async Task<ChatConversationDto?> GetOrCreateConversationAsync(
        int propertyId,
        int buyerId,
        int sellerId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string existingConversationQuery = """
                SELECT
                    ConversationID
                FROM dbo.ChatConversations WITH (UPDLOCK, HOLDLOCK)
                WHERE PropertyID = @PropertyID
                  AND BuyerID = @BuyerID
                  AND SellerID = @SellerID;
                """;

            int? conversationId =
                await connection.QuerySingleOrDefaultAsync<int?>(
                    new CommandDefinition(
                        existingConversationQuery,
                        new
                        {
                            PropertyID = propertyId,
                            BuyerID = buyerId,
                            SellerID = sellerId
                        },
                        transaction: transaction,
                        cancellationToken:
                            cancellationToken));

            if (!conversationId.HasValue)
            {
                const string propertyQuery = """
                    SELECT
                        PropertyID
                    FROM dbo.Properties WITH (UPDLOCK, HOLDLOCK)
                    WHERE PropertyID = @PropertyID
                      AND SellerID = @SellerID
                      AND ListingStatus = 'Approved';
                    """;

                int? approvedPropertyId =
                    await connection.QuerySingleOrDefaultAsync<int?>(
                        new CommandDefinition(
                            propertyQuery,
                            new
                            {
                                PropertyID = propertyId,
                                SellerID = sellerId
                            },
                            transaction: transaction,
                            cancellationToken:
                                cancellationToken));

                if (!approvedPropertyId.HasValue)
                {
                    transaction.Rollback();

                    return null;
                }

                const string insertQuery = """
                    INSERT INTO dbo.ChatConversations
                    (
                        PropertyID,
                        BuyerID,
                        SellerID
                    )
                    OUTPUT INSERTED.ConversationID
                    VALUES
                    (
                        @PropertyID,
                        @BuyerID,
                        @SellerID
                    );
                    """;

                conversationId =
                    await connection.ExecuteScalarAsync<int>(
                        new CommandDefinition(
                            insertQuery,
                            new
                            {
                                PropertyID = propertyId,
                                BuyerID = buyerId,
                                SellerID = sellerId
                            },
                            transaction: transaction,
                            cancellationToken:
                                cancellationToken));
            }

            transaction.Commit();

            return await GetConversationAsync(
                conversationId.Value,
                userId,
                cancellationToken);
        }
        catch
        {
            transaction.Rollback();

            throw;
        }
    }

    public async Task<List<ChatConversationDto>>
        GetBuyerConversationsAsync(
            int buyerId,
            CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                c.ConversationID,
                c.PropertyID,

                p.PropertyGUID,
                p.PropertyTitle,

                c.BuyerID,
                buyer.FullName AS BuyerName,

                c.SellerID,
                seller.FullName AS SellerName,

                lastMessage.MessageText AS LastMessage,
                c.LastMessageAt,

                (
                    SELECT COUNT(*)
                    FROM dbo.ChatMessages cm
                    WHERE cm.ConversationID = c.ConversationID
                      AND cm.ReceiverID = @BuyerID
                      AND cm.IsRead = 0
                ) AS UnreadCount,

                CAST
                (
                    CASE
                        WHEN p.ListingStatus = 'Sold'
                             OR EXISTS
                             (
                                 SELECT 1
                                 FROM dbo.Transactions t
                                 WHERE t.PropertyID = c.PropertyID
                                   AND t.TransactionStatus = 'Completed'
                             )
                        THEN 1
                        ELSE 0
                    END
                    AS BIT
                ) AS IsReadOnly

            FROM dbo.ChatConversations c

            INNER JOIN dbo.Properties p
                ON c.PropertyID = p.PropertyID

            INNER JOIN dbo.Users buyer
                ON c.BuyerID = buyer.UserID

            INNER JOIN dbo.Users seller
                ON c.SellerID = seller.UserID

            OUTER APPLY
            (
                SELECT TOP 1
                    cm.MessageText
                FROM dbo.ChatMessages cm
                WHERE cm.ConversationID = c.ConversationID
                ORDER BY
                    cm.SentAt DESC,
                    cm.MessageID DESC
            ) lastMessage

            WHERE c.BuyerID = @BuyerID

            ORDER BY
                COALESCE(
                    c.LastMessageAt,
                    c.CreatedAt) DESC,
                c.ConversationID DESC;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        IEnumerable<ChatConversationDto> conversations =
            await connection.QueryAsync<ChatConversationDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        BuyerID = buyerId
                    },
                    cancellationToken:
                        cancellationToken));

        return conversations.ToList();
    }

    public async Task<List<ChatConversationDto>>
        GetSellerConversationsAsync(
            int sellerId,
            CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                c.ConversationID,
                c.PropertyID,

                p.PropertyGUID,
                p.PropertyTitle,

                c.BuyerID,
                buyer.FullName AS BuyerName,

                c.SellerID,
                seller.FullName AS SellerName,

                lastMessage.MessageText AS LastMessage,
                c.LastMessageAt,

                (
                    SELECT COUNT(*)
                    FROM dbo.ChatMessages cm
                    WHERE cm.ConversationID = c.ConversationID
                      AND cm.ReceiverID = @SellerID
                      AND cm.IsRead = 0
                ) AS UnreadCount,

                CAST
                (
                    CASE
                        WHEN p.ListingStatus = 'Sold'
                             OR EXISTS
                             (
                                 SELECT 1
                                 FROM dbo.Transactions t
                                 WHERE t.PropertyID = c.PropertyID
                                   AND t.TransactionStatus = 'Completed'
                             )
                        THEN 1
                        ELSE 0
                    END
                    AS BIT
                ) AS IsReadOnly

            FROM dbo.ChatConversations c

            INNER JOIN dbo.Properties p
                ON c.PropertyID = p.PropertyID

            INNER JOIN dbo.Users buyer
                ON c.BuyerID = buyer.UserID

            INNER JOIN dbo.Users seller
                ON c.SellerID = seller.UserID

            OUTER APPLY
            (
                SELECT TOP 1
                    cm.MessageText
                FROM dbo.ChatMessages cm
                WHERE cm.ConversationID = c.ConversationID
                ORDER BY
                    cm.SentAt DESC,
                    cm.MessageID DESC
            ) lastMessage

            WHERE c.SellerID = @SellerID

            ORDER BY
                COALESCE(
                    c.LastMessageAt,
                    c.CreatedAt) DESC,
                c.ConversationID DESC;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        IEnumerable<ChatConversationDto> conversations =
            await connection.QueryAsync<ChatConversationDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        SellerID = sellerId
                    },
                    cancellationToken:
                        cancellationToken));

        return conversations.ToList();
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                cm.MessageID,
                cm.ConversationID,

                c.PropertyID,

                cm.SenderID,
                sender.FullName AS SenderName,

                cm.ReceiverID,
                receiver.FullName AS ReceiverName,

                cm.MessageText,
                cm.SentAt,
                cm.IsRead

            FROM dbo.ChatMessages cm

            INNER JOIN dbo.ChatConversations c
                ON cm.ConversationID = c.ConversationID

            INNER JOIN dbo.Users sender
                ON cm.SenderID = sender.UserID

            INNER JOIN dbo.Users receiver
                ON cm.ReceiverID = receiver.UserID

            WHERE cm.ConversationID = @ConversationID

            ORDER BY
                cm.SentAt ASC,
                cm.MessageID ASC;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        IEnumerable<ChatMessageDto> messages =
            await connection.QueryAsync<ChatMessageDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        ConversationID = conversationId
                    },
                    cancellationToken:
                        cancellationToken));

        return messages.ToList();
    }

    public async Task<ChatMessageDto?> CreateMessageAsync(
        ChatMessage message,
        CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string insertQuery = """
                INSERT INTO dbo.ChatMessages
                (
                    ConversationID,
                    SenderID,
                    ReceiverID,
                    MessageText
                )
                OUTPUT INSERTED.MessageID
                VALUES
                (
                    @ConversationID,
                    @SenderID,
                    @ReceiverID,
                    @MessageText
                );
                """;

            int messageId =
                await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        insertQuery,
                        new
                        {
                            message.ConversationID,
                            message.SenderID,
                            message.ReceiverID,
                            message.MessageText
                        },
                        transaction: transaction,
                        cancellationToken:
                            cancellationToken));

            const string updateConversationQuery = """
                UPDATE dbo.ChatConversations
                SET
                    LastMessageAt = GETUTCDATE()
                WHERE ConversationID = @ConversationID;
                """;

            int rowsAffected =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        updateConversationQuery,
                        new
                        {
                            message.ConversationID
                        },
                        transaction: transaction,
                        cancellationToken:
                            cancellationToken));

            if (rowsAffected != 1)
            {
                throw new InvalidOperationException(
                    "The chat conversation could not be updated.");
            }

            const string messageQuery = """
                SELECT
                    cm.MessageID,
                    cm.ConversationID,

                    c.PropertyID,

                    cm.SenderID,
                    sender.FullName AS SenderName,

                    cm.ReceiverID,
                    receiver.FullName AS ReceiverName,

                    cm.MessageText,
                    cm.SentAt,
                    cm.IsRead

                FROM dbo.ChatMessages cm

                INNER JOIN dbo.ChatConversations c
                    ON cm.ConversationID = c.ConversationID

                INNER JOIN dbo.Users sender
                    ON cm.SenderID = sender.UserID

                INNER JOIN dbo.Users receiver
                    ON cm.ReceiverID = receiver.UserID

                WHERE cm.MessageID = @MessageID;
                """;

            ChatMessageDto? savedMessage =
                await connection.QuerySingleOrDefaultAsync<ChatMessageDto>(
                    new CommandDefinition(
                        messageQuery,
                        new
                        {
                            MessageID = messageId
                        },
                        transaction: transaction,
                        cancellationToken:
                            cancellationToken));

            if (savedMessage == null)
            {
                throw new InvalidOperationException(
                    "The saved chat message could not be retrieved.");
            }

            transaction.Commit();

            return savedMessage;
        }
        catch
        {
            transaction.Rollback();

            throw;
        }
    }

    public async Task<bool> MarkMessagesAsReadAsync(
        int conversationId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string query = """
            UPDATE dbo.ChatMessages
            SET
                IsRead = 1
            WHERE ConversationID = @ConversationID
              AND ReceiverID = @UserID
              AND IsRead = 0;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                new CommandDefinition(
                    query,
                    new
                    {
                        ConversationID = conversationId,
                        UserID = userId
                    },
                    cancellationToken:
                        cancellationToken));

        return rowsAffected > 0;
    }
}