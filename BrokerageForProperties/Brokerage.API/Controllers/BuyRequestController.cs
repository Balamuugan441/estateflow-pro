using Brokerage.Business.Services;
using Brokerage.Models.DTOs.BuyRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Brokerage.API.Controllers;


// Provides Buyer and Seller endpoints for property purchase requests.

[ApiController]
[Route("api/buyrequests")]
[Authorize]
public class BuyRequestController : ControllerBase
{
    private readonly BuyRequestService _buyRequestService;

    public BuyRequestController(
        BuyRequestService buyRequestService)
    {
        _buyRequestService =
            buyRequestService;
    }

    [HttpPost("{propertyGuid:guid}")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> Create(
        Guid propertyGuid)
    {
        if (!TryGetCurrentUserId(out int buyerId))
        {
            return Unauthorized();
        }

        CreateBuyRequestResponse response =
            await _buyRequestService
                .CreateAsync(
                    buyerId,
                    propertyGuid);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("seller")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerRequests(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int sellerId))
        {
            return Unauthorized();
        }

        List<SellerBuyRequestDto> requests =
            await _buyRequestService
                .GetPendingForSellerAsync(
                    sellerId,
                    cancellationToken);

        return Ok(requests);
    }

    [HttpPost("{buyRequestId:int}/reject")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Reject(
        int buyRequestId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int sellerId))
        {
            return Unauthorized();
        }

        bool rejected =
            await _buyRequestService
                .RejectAsync(
                    sellerId,
                    buyRequestId,
                    cancellationToken);

        if (!rejected)
        {
            return NotFound(
                new
                {
                    success = false,
                    message =
                        "Buy request was not found or has already been processed."
                });
        }

        return Ok(
            new
            {
                success = true,
                message =
                    "Buy request rejected."
            });
    }

    private bool TryGetCurrentUserId(
        out int userId)
    {
        string? userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.TryParse(
            userIdClaim,
            out userId);
    }
    [HttpPost("{buyRequestId:int}/approve")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> Approve(
    int buyRequestId,
    CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out int sellerId))
        {
            return Unauthorized();
        }

        ApproveBuyRequestResponse response =
            await _buyRequestService
                .ApproveAsync(
                    sellerId,
                    buyRequestId,
                    cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
