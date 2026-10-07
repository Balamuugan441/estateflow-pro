using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Brokerage.Web.Hubs;

/// <summary>
/// Provides real-time Buyer-Seller chat communication.
/// Messages are persisted through ChatApiClient -> Brokerage.API.
/// </summary>
[Authorize(Roles = "Buyer,Seller")]
public class ChatHub : Hub
{
    private readonly ChatApiClient _chatApiClient;

    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        ChatApiClient chatApiClient,
        ILogger<ChatHub> logger)
    {
        _chatApiClient = chatApiClient;
        _logger = logger;
    }


    /// <summary>
    /// Adds the authenticated connection to its private user group.
    ///
    /// User groups allow the server to deliver messages to all
    /// active browser connections belonging to the same user.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        string? userIdClaim =
            Context.User?
                .FindFirstValue(
                    ClaimTypes.NameIdentifier);

        string? roleClaim =
            Context.User?
                .FindFirstValue(
                    ClaimTypes.Role);

        _logger.LogInformation(
            "SignalR chat connection attempt. ConnectionId: {ConnectionId}, UserID: {UserID}, Role: {Role}",
            Context.ConnectionId,
            userIdClaim,
            roleClaim);

        if (!int.TryParse(
                userIdClaim,
                out int userId))
        {
            _logger.LogWarning(
                "SignalR connection rejected because UserID claim is missing or invalid. ConnectionId: {ConnectionId}",
                Context.ConnectionId);

            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            GetUserGroupName(userId),
            Context.ConnectionAborted);

        _logger.LogInformation(
            "SignalR chat connection established. ConnectionId: {ConnectionId}, UserID: {UserID}, Role: {Role}",
            Context.ConnectionId,
            userId,
            roleClaim);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Sends a message through the existing API layer.
    ///
    /// The authenticated user is never supplied by JavaScript.
    /// Brokerage.API derives SenderID from the authenticated JWT.
    /// </summary>
    public async Task SendMessage(
        int conversationId,
        string messageText)
    {
        if (!TryGetCurrentUserId(out _))
        {
            throw new HubException(
                "The current user could not be identified.");
        }

        if (string.IsNullOrWhiteSpace(messageText))
        {
            throw new HubException(
                "Message cannot be empty.");
        }

        ChatMessageApiModel savedMessage;

        try
        {
            savedMessage =
                await _chatApiClient
                    .SendMessageAsync(
                        conversationId,
                        messageText,
                        Context.ConnectionAborted);
        }
        catch (HttpRequestException)
        {
            throw new HubException(
                "The message could not be sent.");
        }
        catch (ArgumentException)
        {
            throw new HubException(
                "The message could not be sent.");
        }
        catch (InvalidOperationException)
        {
            throw new HubException(
                "The message could not be sent.");
        }



        //Send the persisted message to every active connection
        //belonging to the Sender.

        await Clients
            .Group(
                GetUserGroupName(
                    savedMessage.SenderID))
            .SendAsync(
                "ReceiveMessage",
                savedMessage,
                Context.ConnectionAborted);


        /*
         * Send the same persisted message to every active connection
         * belonging to the Receiver.
         */
        if (savedMessage.ReceiverID !=
            savedMessage.SenderID)
        {
            await Clients
                .Group(
                    GetUserGroupName(
                        savedMessage.ReceiverID))
                .SendAsync(
                    "ReceiveMessage",
                    savedMessage,
                    Context.ConnectionAborted);
        }
        var conversationUpdate = new
        {
            conversationID = savedMessage.ConversationID,
            lastMessage = savedMessage.MessageText,
            lastMessageAt = savedMessage.SentAt,
            senderID = savedMessage.SenderID,
            receiverID = savedMessage.ReceiverID
        };

        await Clients.Group(
                GetUserGroupName(savedMessage.SenderID))
            .SendAsync(
                "ConversationUpdated",
                conversationUpdate,
                Context.ConnectionAborted);

        if (savedMessage.ReceiverID != savedMessage.SenderID)
        {
            await Clients.Group(
                    GetUserGroupName(savedMessage.ReceiverID))
                .SendAsync(
                    "ConversationUpdated",
                    conversationUpdate,
                    Context.ConnectionAborted);
        }
    }


    /// <summary>
    /// Creates the private SignalR group name for a user.
    /// </summary>
    private static string GetUserGroupName(
        int userId)
    {
        return $"chat-user-{userId}";
    }


    /// <summary>
    /// Extracts UserID from the authenticated JWT.
    /// </summary>
    private bool TryGetCurrentUserId(
        out int userId)
    {
        string? userIdClaim =
            Context.User?
                .FindFirstValue(
                    ClaimTypes.NameIdentifier)
            ?? Context.User?
                .FindFirstValue("sub");

        return int.TryParse(
            userIdClaim,
            out userId);
    }
    public async Task MarkConversationAsRead(int conversationId)
    {
        if (!TryGetCurrentUserId(out int userId))
        {
            throw new HubException(
                "The current user could not be identified.");
        }

        try
        {
            await _chatApiClient.MarkMessagesAsReadAsync(
                conversationId,
                Context.ConnectionAborted);
        }
        catch (HttpRequestException)
        {
            throw new HubException(
                "Messages could not be marked as read.");
        }

        await Clients.Group(
                GetUserGroupName(userId))
            .SendAsync(
                "ConversationRead",
                new
                {
                    conversationID = conversationId
                },
                Context.ConnectionAborted);
    }
}