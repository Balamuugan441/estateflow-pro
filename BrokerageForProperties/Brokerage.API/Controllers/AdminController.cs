using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.DTOs.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Brokerage.API.Controllers;


// Administrative management controller for user moderation, property approvals, and system audit logs.

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;

    public AdminController(
        AdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Retrieves a paginated list of registered users with optional filtering parameters.
    /// </summary>
    /// <remarks>
    /// Flow: Validates date parameters -> Queries user repository -> Returns paginated user records.
    /// </remarks>
    /// <param name="request">Search, role filter, active status, date range, and pagination parameters.</param>
    /// <returns>Paginated list of system users.</returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] AdminUserQueryRequest request)
    {
        try
        {
            AdminUserListResponse response =
                await _adminService
                    .GetUsersAsync(request);

            return Ok(response);
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
    }

    // Updates a user's active status for administrator user management.
    [HttpPut("users/{userId:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int userId, [FromBody] AdminUserStatusRequest request)
    {
        AdminUserStatusResponse response = await _adminService.UpdateUserStatusAsync(userId, request.IsActive);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a paginated list of properties with filters for status, type, and price range.
    /// </summary>
    /// <remarks>
    /// Flow: Validates price boundaries -> Queries property repository -> Returns matching property listings.
    /// </remarks>
    /// <param name="request">Property search and filter parameters.</param>
    /// <returns>Paginated list of property listings with status counts.</returns>
    [HttpGet("properties")]
    public async Task<IActionResult> GetProperties(
        [FromQuery] AdminPropertyQueryRequest request)
    {
        try
        {
            AdminPropertyListResponse response =
                await _adminService
                    .GetPropertiesAsync(request);

            return Ok(response);
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
    }

    /// <summary>
    /// Retrieves a paginated list of submitted properties pending admin approval.
    /// </summary>
    /// <remarks>
    /// Flow: Queries properties with 'Pending' status ordered by submission time -> Returns pending approval list.
    /// </remarks>
    /// <param name="pageNumber">Current page index (defaults to 1).</param>
    /// <returns>Paginated collection of pending property approvals.</returns>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(
[FromQuery] int pageNumber = 1)
    {
        AdminPendingApprovalListResponse response =
            await _adminService.GetPendingApprovalsAsync(pageNumber);

        return Ok(response);
    }

    /// <summary>
    /// Updates the approval status of a pending property listing (Approved or Rejected).
    /// </summary>
    /// <remarks>
    /// Flow: Validates property existence and pending status -> Updates listing status -> Logs admin action.
    /// </remarks>
    /// <param name="propertyId">Target property ID.</param>
    /// <param name="request">Status update payload ('Approved' or 'Rejected').</param>
    /// <returns>Action result response.</returns>
    [HttpPut(
    "pending-approvals/{propertyId:int}/status")]
    public async Task<IActionResult>
    UpdatePendingPropertyStatus(
        int propertyId,
        [FromBody] AdminPropertyStatusRequest request)
    {
        AdminPropertyActionResponse response =
            await _adminService
                .UpdatePendingPropertyStatusAsync(
                    propertyId,
                    request.ListingStatus);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Retrieves full property details, amenities, and uploaded media for admin review before approval.
    /// </summary>
    /// <remarks>
    /// Flow: Fetches pending property -> Retrieves associated amenities and media files -> Returns combined review preview.
    /// </remarks>
    /// <param name="propertyId">Target property ID.</param>
    /// <returns>Complete property preview details.</returns>
    [HttpGet(
    "pending-approvals/{propertyId:int}/preview")]
    public async Task<IActionResult>
    GetPropertyPreview(
        int propertyId)
    {
        PropertyReviewResponse? response =
            await _adminService
                .GetPropertyPreviewAsync(propertyId);

        if (response == null)
        {
            return NotFound(
                new
                {
                    success = false,
                    message =
                        "Pending property was not found."
                });
        }

        return Ok(response);
    }

    /// <summary>
    /// Retrieves key system statistics and metrics for the admin dashboard.
    /// </summary>
    /// <remarks>
    /// Flow: Aggregates total metrics (users, listings, approvals, revenue) -> Returns dashboard model.
    /// </remarks>
    /// <returns>Admin dashboard overview metrics.</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult>
    GetDashboard()
    {
        AdminDashboardResponse response =
            await _adminService
                .GetDashboardAsync();

        return Ok(response);
    }

    /// <summary>
    /// Fetches system activity and audit logs for administrative tracking.
    /// </summary>
    /// <remarks>
    /// Flow: Validates date filters -> Queries activity log records -> Returns paginated log entries.
    /// </remarks>
    /// <param name="request">Activity log filter and pagination criteria.</param>
    /// <returns>Paginated activity log records.</returns>
    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs(
    [FromQuery] AdminActivityLogQueryRequest request)
    {
        AdminActivityLogListResponse response =
            await _adminService
                .GetActivityLogsAsync(request);

        return Ok(response);
    }
}