using Brokerage.Models.DTOs.BuyRequests;

namespace Brokerage.Data.Interfaces;

// Provides database operations for Buyer purchase requests.

public interface IBuyRequestRepository
{
    Task<int?> CreateAsync(
        int buyerId,
        Guid propertyGuid);

    Task<List<SellerBuyRequestDto>> GetPendingForSellerAsync(
        int sellerId,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(
        int sellerId,
        int buyRequestId,
        CancellationToken cancellationToken = default);
    Task<ApproveBuyRequestResponse> ApproveAsync(
    int sellerId,
    int buyRequestId,
    CancellationToken cancellationToken = default);
}