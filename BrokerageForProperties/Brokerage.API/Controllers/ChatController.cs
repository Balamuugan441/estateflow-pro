using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Brokerage.API.Controllers;

/// <summary>
/// Provides Buyer and Seller endpoints for property-specific chat.
/// </summary>
[ApiController]
[Route("api/chat")]
[Authorize(Roles = "Buyer,Seller")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(
        ChatService chatService)
    {
        _chatService = chatService;
    }


    /// <summary>
    /// Starts or retrieves a conversation for the authenticated Buyer
    /// with the Seller of the specified approved property.
    /// </summary>
    [HttpPost("conversations")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> StartConversation(
        StartChatRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out int userId, out _))
        {
            return Unauthorized();
        }

        ChatConversationDto? conversation =
            await _chatService.StartConversationAsync(
                userId,
                request.PropertyGUID,
                cancellationToken);

        if (conversation == null)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message =
                        "Chat can only be started for an existing approved property."
                });
        }

        return Ok(conversation);
    }


    /// <summary>
    /// Gets all conversations belonging to the authenticated Buyer.
    /// </summary>
    [HttpGet("conversations/buyer")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> GetBuyerConversations(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out int buyerId, out _))
        {
            return Unauthorized();
        }

        List<ChatConversationDto> conversations =
            await _chatService
                .GetBuyerConversationsAsync(
                    buyerId,
                    cancellationToken);

        return Ok(conversations);
    }


    /// <summary>
    /// Gets all conversations belonging to the authenticated Seller.
    /// </summary>
    [HttpGet("conversations/seller")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerConversations(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(out int sellerId, out _))
        {
            return Unauthorized();
        }

        List<ChatConversationDto> conversations =
            await _chatService
                .GetSellerConversationsAsync(
                    sellerId,
                    cancellationToken);

        return Ok(conversations);
    }


    /// <summary>
    /// Gets a specific conversation if the authenticated user
    /// is a participant in it.
    /// </summary>
    [HttpGet("conversations/{conversationId:int}")]
    public async Task<IActionResult> GetConversation(
        int conversationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(
                out int userId,
                out string? role))
        {
            return Unauthorized();
        }

        ChatConversationDto? conversation =
            await _chatService
                .GetConversationAsync(
                    conversationId,
                    userId,
                    role!,
                    cancellationToken);

        if (conversation == null)
        {
            return NotFound(
                new
                {
                    success = false,
                    message = "Conversation not found."
                });
        }

        return Ok(conversation);
    }


    /// <summary>
    /// Gets persisted messages for a conversation.
    /// </summary>
    [HttpGet("conversations/{conversationId:int}/messages")]
    public async Task<IActionResult> GetMessages(
     int conversationId,
     CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(
                out int userId,
                out string? role))
        {
            return Unauthorized();
        }

        ChatConversationDto? conversation =
            await _chatService
                .GetConversationAsync(
                    conversationId,
                    userId,
                    role!,
                    cancellationToken);

        if (conversation == null)
        {
            return NotFound(
                new
                {
                    success = false,
                    message = "Conversation not found."
                });
        }

        List<ChatMessageDto> messages =
            await _chatService
                .GetMessagesAsync(
                    conversationId,
                    userId,
                    role!,
                    cancellationToken);

        return Ok(messages);
    }


    /// <summary>
    /// Sends and persists a message inside an existing conversation.
    /// SenderID and ReceiverID are always determined server-side.
    /// </summary>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage(
        SendChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(
                out int userId,
                out string? role))
        {
            return Unauthorized();
        }

        try
        {
            ChatMessageDto message =
                await _chatService
                    .SendMessageAsync(
                        userId,
                        role!,
                        request,
                        cancellationToken);

            return Ok(message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message = ex.Message
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    success = false,
                    message = ex.Message
                });
        }
    }


    /// <summary>
    /// Marks messages received by the authenticated user
    /// in the specified conversation as read.
    /// </summary>
    [HttpPost("conversations/{conversationId:int}/read")]
    public async Task<IActionResult> MarkMessagesAsRead(
        int conversationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUser(
                out int userId,
                out string? role))
        {
            return Unauthorized();
        }

        bool marked =
            await _chatService
                .MarkMessagesAsReadAsync(
                    conversationId,
                    userId,
                    role!,
                    cancellationToken);

        if (!marked)
        {
            return NotFound(
                new
                {
                    success = false,
                    message = "Conversation not found."
                });
        }

        return Ok(
            new
            {
                success = true,
                message = "Messages marked as read."
            });
    }


    /// <summary>
    /// Extracts the authenticated user ID and role from JWT claims.
    /// </summary>
    private bool TryGetCurrentUser(
        out int userId,
        out string? role)
    {
        string? userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        role =
            User.FindFirstValue(
                ClaimTypes.Role)
            ?? User.FindFirstValue("role");

        return int.TryParse(
            userIdClaim,
            out userId);
    }
}