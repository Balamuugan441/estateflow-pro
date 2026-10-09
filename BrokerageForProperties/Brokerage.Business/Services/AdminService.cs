using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.DTOs.Properties;
using Brokerage.Models.Entities;
using Brokerage.Models.Enums;

namespace Brokerage.Business.Services;

public class AdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IAdminDashboardRepository _adminDashboardRepository;
    private readonly ActivityLogService _activityLogService;
    public AdminService(
        IUserRepository userRepository,
        IPropertyRepository propertyRepository,
        IAdminDashboardRepository adminDashboardRepository,
        ActivityLogService activityLogService)
    {
        _userRepository = userRepository;
        _propertyRepository = propertyRepository;
        _adminDashboardRepository = adminDashboardRepository;
        _activityLogService = activityLogService;
    }

    public async Task<AdminUserListResponse>
        GetUsersAsync(
            AdminUserQueryRequest request)
    {
        if (request.PageNumber < 1)
        {
            request.PageNumber = 1;
        }

        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate.Value.Date >
            request.ToDate.Value.Date)
        {
            throw new ArgumentException(
                "From date cannot be after To date.");
        }

        return await _userRepository
            .GetUsersForAdminAsync(request);
    }

    public async Task<AdminPropertyListResponse>
        GetPropertiesAsync(
            AdminPropertyQueryRequest request)
    {
        if (request.PageNumber < 1)
        {
            request.PageNumber = 1;
        }

        if (request.MinPrice.HasValue &&
            request.MaxPrice.HasValue &&
            request.MinPrice.Value >
            request.MaxPrice.Value)
        {
            throw new ArgumentException(
                "Minimum price cannot be greater than maximum price.");
        }

        return await _propertyRepository
            .GetPropertiesForAdminAsync(request);
    }
    public async Task<AdminPendingApprovalListResponse>
  GetPendingApprovalsAsync(
      int pageNumber)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        return await _propertyRepository
            .GetPendingApprovalsAsync(pageNumber);
    }
    public async Task<AdminPropertyActionResponse>
    UpdatePendingPropertyStatusAsync(
        int propertyId,
        string listingStatus)
    {
        if (listingStatus != "Approved" &&
            listingStatus != "Rejected")
        {
            return new AdminPropertyActionResponse
            {
                Success = false,
                Message = "Invalid property status."
            };
        }

        Property? property =
            await _propertyRepository
                .GetPropertyByIdAsync(propertyId);

        if (property == null)
        {
            return new AdminPropertyActionResponse
            {
                Success = false,
                Message = "Property not found."
            };
        }

        if (property.ListingStatus != "Pending")
        {
            return new AdminPropertyActionResponse
            {
                Success = false,
                Message =
                    "Only pending properties can be approved or rejected."
            };
        }

        bool updated =
            await _propertyRepository
                .UpdatePendingPropertyStatusAsync(
                    propertyId,
                    listingStatus);

        if (!updated)
        {
            return new AdminPropertyActionResponse
            {
                Success = false,
                Message = "Unable to update property status."
            };
        }

        string actionType =
            listingStatus == "Approved"
                ? "PROPERTY_APPROVED"
                : "PROPERTY_REJECTED";

        string details =
            listingStatus == "Approved"
                ? "Property was approved by administrator."
                : "Property was rejected by administrator.";

        await _activityLogService.LogCurrentUserAsync(
            actionType,
            "ADMIN",
            "PROPERTY",
            propertyId,
            property.PropertyTitle,
            "Success",
            $"Pending → {listingStatus}. {details}");

        return new AdminPropertyActionResponse
        {
            Success = true,
            Message =
                listingStatus == "Approved"
                    ? "Property approved successfully."
                    : "Property rejected successfully."
        };
    }
    public async Task<PropertyReviewResponse?>
    GetPropertyPreviewAsync(
        int propertyId)
    {
        Property? property =
            await _propertyRepository
                .GetPropertyByIdAsync(propertyId);

        if (property == null)
        {
            return null;
        }

        if (property.ListingStatus != "Pending")
        {
            return null;
        }

        IEnumerable<Amenity> amenities =
            await _propertyRepository
                .GetPropertyAmenitiesAsync(propertyId);

        IEnumerable<PropertyMedia> media =
            await _propertyRepository
                .GetPropertyMediaAsync(propertyId);

        return new PropertyReviewResponse
        {
            Property = property,
            Amenities = amenities,
            Media = media
        };
    }
    public async Task<AdminDashboardResponse>
    GetDashboardAsync()
    {
        return await _adminDashboardRepository
            .GetDashboardAsync();
    }
    public async Task<AdminActivityLogListResponse>
    GetActivityLogsAsync(
        AdminActivityLogQueryRequest request)
    {
        if (request.PageNumber < 1)
        {
            request.PageNumber = 1;
        }

        if (request.FromDate.HasValue &&
            request.ToDate.HasValue &&
            request.FromDate > request.ToDate)
        {
            throw new ArgumentException(
                "From date cannot be later than To date.");
        }

        return await _activityLogService
            .GetActivityLogsForAdminAsync(request);
    }
    // Updates a user's active status and returns the new state for the Admin Users page.
    public async Task<AdminUserStatusResponse> UpdateUserStatusAsync(int userId, bool isActive)


    {
        UserStatus status =
            isActive
                ? UserStatus.Active
                : UserStatus.Inactive;

        bool updated =
            await _userRepository.UpdateUserStatusAsync(
                userId,
                status);

        if (!updated)
        {
            return new AdminUserStatusResponse
            {
                Success = false,
                Message = "User not found.",
                UserID = userId,
                IsActive = isActive
            };
        }

        return new AdminUserStatusResponse
        {
            Success = true,
            Message =
                status == UserStatus.Active
                    ? "User unblocked successfully."
                    : "User blocked successfully.",
            UserID = userId,
            IsActive = isActive
        };
    }

}