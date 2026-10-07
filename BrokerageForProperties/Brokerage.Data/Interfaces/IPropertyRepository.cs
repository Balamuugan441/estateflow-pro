using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.DTOs.Home;
using Brokerage.Models.DTOs.Properties;
using Brokerage.Models.Entities;

namespace Brokerage.Data.Interfaces;

public interface IPropertyRepository
{
    Task<int> CreatePropertyAsync(Property property);

    Task<Property?> GetPropertyByIdAsync(int propertyId);
    Task<Property?> GetPropertyByGuidAsync(Guid propertyGuid);

    Task<IEnumerable<Property>> GetPropertiesBySellerAsync(int sellerId);
    Task<bool> UpdatePropertyAsync(Property property);

    Task<bool> UpdateListingStatusAsync(int propertyId, string listingStatus);
    Task<IEnumerable<Amenity>> GetAllAmenitiesAsync();

    Task<bool> SavePropertyAmenitiesAsync(int propertyId, IEnumerable<int> amenityIds);
    Task<bool> SavePropertyMediaAsync(IEnumerable<PropertyMedia> media);
    Task<IEnumerable<PropertyMedia>> GetPropertyMediaAsync(int propertyId);
    Task<IEnumerable<Amenity>> GetPropertyAmenitiesAsync(int propertyId);
    Task<AdminPropertyListResponse> GetPropertiesForAdminAsync(AdminPropertyQueryRequest request);
    Task<AdminPendingApprovalListResponse> GetPendingApprovalsAsync(int pageNumber);

    Task<bool> UpdatePendingPropertyStatusAsync(int propertyId, string listingStatus);
    Task<int> GetApprovedPropertyCountAsync();

    Task<IEnumerable<PublicPropertyResponse>>
        GetLatestApprovedPropertiesAsync(
            string? city,
            int count);
    Task<BuyerPropertyListResponse>
    SearchApprovedPropertiesAsync(
        BuyerPropertySearchRequest request,
        int buyerId,
        CancellationToken cancellationToken = default);
}