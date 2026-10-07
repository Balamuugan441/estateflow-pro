using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.BuyRequests;
using Dapper;
using System.Data;
namespace Brokerage.Data.Repositories;


// Handles database operations for Buyer purchase requests.

public class BuyRequestRepository : IBuyRequestRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public BuyRequestRepository(
        ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int?> CreateAsync(
     int buyerId,
     Guid propertyGuid)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string propertyQuery = """
            SELECT
                p.PropertyID,
                p.SellerID,
                p.PropertyTitle,
                p.Price,

                buyer.FullName AS BuyerName,
                seller.FullName AS SellerName

            FROM dbo.Properties p WITH (UPDLOCK, HOLDLOCK)

            INNER JOIN dbo.Users buyer
                ON buyer.UserID = @BuyerID

            INNER JOIN dbo.Users seller
                ON seller.UserID = p.SellerID

            WHERE p.PropertyGUID = @PropertyGUID
              AND p.ListingStatus = 'Approved'

              AND NOT EXISTS
              (
                  SELECT 1
                  FROM dbo.BuyRequests br
                  WHERE br.BuyerID = @BuyerID
                    AND br.PropertyID = p.PropertyID
                    AND br.RequestStatus = 'Pending'
              );
            """;

            BuyRequestCreateData? property =
                await connection
                    .QuerySingleOrDefaultAsync<BuyRequestCreateData>(
                        new CommandDefinition(
                            propertyQuery,
                            new
                            {
                                BuyerID = buyerId,
                                PropertyGUID = propertyGuid
                            },
                            transaction: transaction));

            if (property == null)
            {
                transaction.Rollback();

                return null;
            }


            const string requestQuery = """
            INSERT INTO dbo.BuyRequests
            (
                PropertyID,
                BuyerID,
                SellerID
            )
            OUTPUT INSERTED.BuyRequestID
            VALUES
            (
                @PropertyID,
                @BuyerID,
                @SellerID
            );
            """;

            int buyRequestId =
                await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        requestQuery,
                        new
                        {
                            PropertyID =
                                property.PropertyID,

                            BuyerID =
                                buyerId,

                            SellerID =
                                property.SellerID
                        },
                        transaction: transaction));


            string notificationMessage =
                $"{property.BuyerName} has requested to purchase " +
                $"'{property.PropertyTitle}' for " +
                $"₹{property.Price:N2}.";

            await CreateNotificationAsync(
                connection,
                transaction,
                property.SellerID,
                buyerId,
                "BuyRequest",
                "New Buy Request",
                notificationMessage,
                "BuyRequest",
                buyRequestId);


            transaction.Commit();

            return buyRequestId;
        }
        catch
        {
            transaction.Rollback();

            throw;
        }
    }
    public async Task<List<SellerBuyRequestDto>>
        GetPendingForSellerAsync(
            int sellerId,
            CancellationToken cancellationToken = default)
    {
        const string query = """
            SELECT
                br.BuyRequestID,

                p.PropertyID,
                p.PropertyGUID,
                p.PropertyTitle,
                p.PropertyType,
                p.ListingType,
                p.LocationAddress,
                p.City,
                p.State,
                p.Price,
                p.Area,
                p.AreaUnit,
                p.Bedrooms,
                p.Bathrooms,

                cover.FilePath AS CoverImagePath,

                b.UserID AS BuyerID,
                b.FullName AS BuyerName,
                b.Email AS BuyerEmail,
                b.MobileNumber AS BuyerMobileNumber,

                br.RequestedAt,
                br.RequestStatus

            FROM dbo.BuyRequests br

            INNER JOIN dbo.Properties p
                ON br.PropertyID = p.PropertyID

            INNER JOIN dbo.Users b
                ON br.BuyerID = b.UserID

            OUTER APPLY
            (
                SELECT TOP 1
                    pm.FilePath
                FROM dbo.PropertyMedia pm
                WHERE pm.PropertyID = p.PropertyID
                  AND pm.MediaType = 'CoverPhoto'
                ORDER BY
                    CASE
                        WHEN pm.DisplayOrder IS NULL
                        THEN 999
                        ELSE pm.DisplayOrder
                    END,
                    pm.MediaID
            ) cover

            WHERE br.SellerID = @SellerID
              AND br.RequestStatus = 'Pending'
              AND p.ListingStatus = 'Approved'

            ORDER BY
                br.RequestedAt DESC,
                br.BuyRequestID DESC;
            """;

        using var connection =
            _connectionFactory.CreateConnection();

        IEnumerable<SellerBuyRequestDto> requests =
            await connection.QueryAsync<SellerBuyRequestDto>(
                new CommandDefinition(
                    query,
                    new
                    {
                        SellerID = sellerId
                    },
                    cancellationToken: cancellationToken));

        return requests.ToList();
    }

    public async Task<bool> RejectAsync(
    int sellerId,
    int buyRequestId,
    CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string requestQuery = """
            SELECT
                br.BuyRequestID,
                br.BuyerID,
                br.SellerID,

                p.PropertyTitle,
                p.Price,

                buyer.FullName AS BuyerName,
                seller.FullName AS SellerName

            FROM dbo.BuyRequests br WITH (UPDLOCK, HOLDLOCK)

            INNER JOIN dbo.Properties p
                ON br.PropertyID = p.PropertyID

            INNER JOIN dbo.Users buyer
                ON buyer.UserID = br.BuyerID

            INNER JOIN dbo.Users seller
                ON seller.UserID = br.SellerID

            WHERE br.BuyRequestID = @BuyRequestID
              AND br.SellerID = @SellerID
              AND br.RequestStatus = 'Pending';
            """;

            BuyRequestRejectData? request =
                await connection
                    .QuerySingleOrDefaultAsync<BuyRequestRejectData>(
                        new CommandDefinition(
                            requestQuery,
                            new
                            {
                                BuyRequestID =
                                    buyRequestId,

                                SellerID =
                                    sellerId
                            },
                            transaction: transaction,
                            cancellationToken:
                                cancellationToken));

            if (request == null)
            {
                transaction.Rollback();

                return false;
            }


            const string updateQuery = """
            UPDATE dbo.BuyRequests
            SET
                RequestStatus = 'Rejected',
                RespondedAt = GETUTCDATE()
            WHERE BuyRequestID = @BuyRequestID
              AND SellerID = @SellerID
              AND RequestStatus = 'Pending';
            """;

            int rowsAffected =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        updateQuery,
                        new
                        {
                            BuyRequestID =
                                buyRequestId,

                            SellerID =
                                sellerId
                        },
                        transaction: transaction,
                        cancellationToken:
                            cancellationToken));

            if (rowsAffected != 1)
            {
                throw new InvalidOperationException(
                    "The buy request could not be rejected.");
            }


            string notificationMessage =
                $"Your buy request for '{request.PropertyTitle}' " +
                $"was rejected by {request.SellerName}. " +
                $"The property is still available and you may submit " +
                $"a new request.";

            await CreateNotificationAsync(
                connection,
                transaction,
                request.BuyerID,
                sellerId,
                "BuyRequestRejected",
                "Buy Request Rejected",
                notificationMessage,
                "BuyRequest",
                buyRequestId);


            transaction.Commit();

            return true;
        }
        catch
        {
            transaction.Rollback();

            throw;
        }
    }
    public async Task<ApproveBuyRequestResponse> ApproveAsync(
    int sellerId,
    int buyRequestId,
    CancellationToken cancellationToken = default)
    {
        using var connection =
            _connectionFactory.CreateConnection();

        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string requestQuery = """
            SELECT
                br.BuyRequestID,
                br.PropertyID,
                br.BuyerID,
                br.SellerID,
                br.RequestStatus,

                p.PropertyGUID,
                p.PropertyTitle,
                p.LocationAddress,
                p.Price,
                p.ListingStatus,

                buyer.FullName AS BuyerName,
                seller.FullName AS SellerName

            FROM dbo.BuyRequests br WITH (UPDLOCK, HOLDLOCK)

            INNER JOIN dbo.Properties p WITH (UPDLOCK, HOLDLOCK)
                ON br.PropertyID = p.PropertyID

            INNER JOIN dbo.Users buyer
                ON br.BuyerID = buyer.UserID

            INNER JOIN dbo.Users seller
                ON br.SellerID = seller.UserID

            WHERE br.BuyRequestID = @BuyRequestID
              AND br.SellerID = @SellerID;
            """;

            BuyRequestApprovalData? request =
                await connection.QuerySingleOrDefaultAsync<BuyRequestApprovalData>(
                    new CommandDefinition(
                        requestQuery,
                        new
                        {
                            BuyRequestID = buyRequestId,
                            SellerID = sellerId
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

            if (request == null)
            {
                transaction.Rollback();

                return new ApproveBuyRequestResponse
                {
                    Success = false,
                    Message =
                        "Buy request was not found for this Seller."
                };
            }

            if (!string.Equals(
                    request.RequestStatus,
                    "Pending",
                    StringComparison.OrdinalIgnoreCase))
            {
                transaction.Rollback();

                return new ApproveBuyRequestResponse
                {
                    Success = false,
                    Message =
                        "This buy request has already been processed."
                };
            }

            if (!string.Equals(
                    request.ListingStatus,
                    "Approved",
                    StringComparison.OrdinalIgnoreCase))
            {
                transaction.Rollback();

                return new ApproveBuyRequestResponse
                {
                    Success = false,
                    Message =
                        "This property is no longer available."
                };
            }
            const string otherRequestsQuery = """
    SELECT
        br.BuyRequestID,
        br.BuyerID,
        buyer.FullName AS BuyerName

    FROM dbo.BuyRequests br WITH (UPDLOCK, HOLDLOCK)

    INNER JOIN dbo.Users buyer
        ON buyer.UserID = br.BuyerID

    WHERE br.PropertyID = @PropertyID
      AND br.BuyRequestID <> @BuyRequestID
      AND br.RequestStatus = 'Pending';
    """;

            List<PendingBuyRequestData> otherRequests =
                (
                    await connection.QueryAsync<PendingBuyRequestData>(
                        new CommandDefinition(
                            otherRequestsQuery,
                            new
                            {
                                PropertyID =
                                    request.PropertyID,

                                BuyRequestID =
                                    request.BuyRequestID
                            },
                            transaction: transaction,
                            cancellationToken:
                                cancellationToken))
                ).ToList();

            const string transactionQuery = """
            INSERT INTO dbo.Transactions
            (
                BuyRequestID,
                PropertyID,
                PropertyGUID,
                PropertyTitle,
                PropertyLocation,
                BuyerID,
                BuyerName,
                SellerID,
                SellerName,
                TransactionAmount,
                TransactionStatus
            )
            OUTPUT INSERTED.TransactionID
            VALUES
            (
                @BuyRequestID,
                @PropertyID,
                @PropertyGUID,
                @PropertyTitle,
                @PropertyLocation,
                @BuyerID,
                @BuyerName,
                @SellerID,
                @SellerName,
                @TransactionAmount,
                'Completed'
            );
            """;

            int transactionId =
                await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(
                        transactionQuery,
                        new
                        {
                            BuyRequestID =
                                request.BuyRequestID,

                            PropertyID =
                                request.PropertyID,

                            PropertyGUID =
                                request.PropertyGUID,

                            PropertyTitle =
                                request.PropertyTitle,

                            PropertyLocation =
                                request.LocationAddress,

                            BuyerID =
                                request.BuyerID,

                            BuyerName =
                                request.BuyerName,

                            SellerID =
                                request.SellerID,

                            SellerName =
                                request.SellerName,

                            TransactionAmount =
                                request.Price
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));


            const string approveRequestQuery = """
            UPDATE dbo.BuyRequests
            SET
                RequestStatus = 'Approved',
                RespondedAt = GETUTCDATE()
            WHERE BuyRequestID = @BuyRequestID
              AND SellerID = @SellerID
              AND RequestStatus = 'Pending';
            """;

            int approvedRows =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        approveRequestQuery,
                        new
                        {
                            BuyRequestID = buyRequestId,
                            SellerID = sellerId
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

            if (approvedRows != 1)
            {
                throw new InvalidOperationException(
                    "The buy request could not be approved.");
            }


            const string rejectOtherRequestsQuery = """
            UPDATE dbo.BuyRequests
            SET
                RequestStatus = 'Rejected',
                RespondedAt = GETUTCDATE()
            WHERE PropertyID = @PropertyID
              AND BuyRequestID <> @BuyRequestID
              AND RequestStatus = 'Pending';
            """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    rejectOtherRequestsQuery,
                    new
                    {
                        PropertyID =
                            request.PropertyID,

                        BuyRequestID =
                            request.BuyRequestID
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));


            const string completePropertyQuery = """
            UPDATE dbo.Properties
            SET
                ListingStatus = 'Sold',
                UpdatedAt = GETUTCDATE()
            WHERE PropertyID = @PropertyID
              AND ListingStatus = 'Approved';
            """;

            int propertyRows =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        completePropertyQuery,
                        new
                        {
                            PropertyID =
                                request.PropertyID
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

            if (propertyRows != 1)
            {
                throw new InvalidOperationException(
                    "The property could not be marked as completed.");
            }


            const string activityLogQuery = """
            INSERT INTO dbo.ActivityLogs
            (
                ActorUserID,
                ActorName,
                ActorRole,
                ActionType,
                ActionCategory,
                EntityType,
                EntityID,
                EntityName,
                Result,
                Details
            )
            VALUES
            (
                @ActorUserID,
                @ActorName,
                @ActorRole,
                'DEAL_APPROVED',
                'TRANSACTION',
                'Transaction',
                @EntityID,
                @EntityName,
                'Success',
                @Details
            );
            """;

            string details =
                $"Buy request {request.BuyRequestID} was approved. " +
                $"Buyer {request.BuyerName} purchased " +
                $"property '{request.PropertyTitle}' " +
                $"from Seller {request.SellerName}.";

            await connection.ExecuteAsync(
                new CommandDefinition(
                    activityLogQuery,
                    new
                    {
                        ActorUserID =
                            sellerId,

                        ActorName =
                            request.SellerName,

                        ActorRole =
                            "Seller",

                        EntityID =
                            transactionId,

                        EntityName =
                            request.PropertyTitle,

                        Details =
                            details
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));
            string approvedBuyerMessage =
    $"Your buy request for '{request.PropertyTitle}' " +
    $"has been approved by {request.SellerName}. " +
    $"Transaction #{transactionId} has been completed.";

            await CreateNotificationAsync(
                connection,
                transaction,
                request.BuyerID,
                sellerId,
                "BuyRequestApproved",
                "Buy Request Approved",
                approvedBuyerMessage,
                "Transaction",
                transactionId);


            foreach (
                PendingBuyRequestData otherRequest
                in otherRequests)
            {
                string rejectedBuyerMessage =
                    $"Your buy request for '{request.PropertyTitle}' " +
                    $"was not approved because another Buyer's request " +
                    $"was accepted by the Seller.";

                await CreateNotificationAsync(
                    connection,
                    transaction,
                    otherRequest.BuyerID,
                    sellerId,
                    "BuyRequestRejected",
                    "Buy Request Rejected",
                    rejectedBuyerMessage,
                    "BuyRequest",
                    otherRequest.BuyRequestID);
            }


            transaction.Commit();

            return new ApproveBuyRequestResponse
            {
                Success = true,
                Message =
                    "Deal approved successfully.",
                TransactionID =
                    transactionId
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
    private static async Task CreateNotificationAsync(
    IDbConnection connection,
    IDbTransaction transaction,
    int recipientUserId,
    int senderUserId,
    string notificationType,
    string title,
    string message,
    string relatedEntityType,
    int relatedEntityId)
    {
        const string query = """
        INSERT INTO dbo.Notifications
        (
            RecipientUserID,
            SenderUserID,
            NotificationType,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityID,
            IsRead
        )
        VALUES
        (
            @RecipientUserID,
            @SenderUserID,
            @NotificationType,
            @Title,
            @Message,
            @RelatedEntityType,
            @RelatedEntityID,
            0
        );
        """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                query,
                new
                {
                    RecipientUserID =
                        recipientUserId,

                    SenderUserID =
                        senderUserId,

                    NotificationType =
                        notificationType,

                    Title =
                        title,

                    Message =
                        message,

                    RelatedEntityType =
                        relatedEntityType,

                    RelatedEntityID =
                        relatedEntityId
                },
                transaction: transaction));
    }
    private sealed class BuyRequestApprovalData
    {
        public int BuyRequestID { get; set; }

        public int PropertyID { get; set; }

        public int BuyerID { get; set; }

        public int SellerID { get; set; }

        public string RequestStatus { get; set; } = string.Empty;

        public Guid PropertyGUID { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public string? LocationAddress { get; set; }

        public decimal Price { get; set; }

        public string ListingStatus { get; set; } = string.Empty;

        public string BuyerName { get; set; } = string.Empty;

        public string SellerName { get; set; } = string.Empty;
    }
    private sealed class BuyRequestCreateData
    {
        public int PropertyID { get; set; }

        public int SellerID { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string BuyerName { get; set; } = string.Empty;

        public string SellerName { get; set; } = string.Empty;
    }
    private sealed class BuyRequestRejectData
    {
        public int BuyRequestID { get; set; }

        public int BuyerID { get; set; }

        public int SellerID { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string BuyerName { get; set; } = string.Empty;

        public string SellerName { get; set; } = string.Empty;
    }
    private sealed class PendingBuyRequestData
    {
        public int BuyRequestID { get; set; }

        public int BuyerID { get; set; }

        public string BuyerName { get; set; } = string.Empty;
    }
}
