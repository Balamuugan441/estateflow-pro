using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.BuyRequests;

namespace Brokerage.Business.Services;

// Handles Buyer purchase-request operations and Seller request retrieval.

public class BuyRequestService
{
    private readonly IBuyRequestRepository _buyRequestRepository;

    public BuyRequestService(
        IBuyRequestRepository buyRequestRepository)
    {
        _buyRequestRepository =
            buyRequestRepository;
    }

    public async Task<CreateBuyRequestResponse>
        CreateAsync(
            int buyerId,
            Guid propertyGuid)
    {
        int? buyRequestId =
            await _buyRequestRepository
                .CreateAsync(
                    buyerId,
                    propertyGuid);

        if (!buyRequestId.HasValue)
        {
            return new CreateBuyRequestResponse
            {
                Success = false,
                Message =
                    "The property is unavailable, not approved, or you have already sent a buy request."
            };
        }

        return new CreateBuyRequestResponse
        {
            Success = true,
            Message =
                "Buy request has been sent to the seller.",
            BuyRequestID =
                buyRequestId.Value
        };
    }

    public async Task<List<SellerBuyRequestDto>>
        GetPendingForSellerAsync(
            int sellerId,
            CancellationToken cancellationToken = default)
    {
        return await _buyRequestRepository
            .GetPendingForSellerAsync(
                sellerId,
                cancellationToken);
    }

    public async Task<bool> RejectAsync(
        int sellerId,
        int buyRequestId,
        CancellationToken cancellationToken = default)
    {
        return await _buyRequestRepository
            .RejectAsync(
                sellerId,
                buyRequestId,
                cancellationToken);
    }
    public async Task<ApproveBuyRequestResponse>
    ApproveAsync(
        int sellerId,
        int buyRequestId,
        CancellationToken cancellationToken = default)
    {
        return await _buyRequestRepository
            .ApproveAsync(
                sellerId,
                buyRequestId,
                cancellationToken);
    }
}