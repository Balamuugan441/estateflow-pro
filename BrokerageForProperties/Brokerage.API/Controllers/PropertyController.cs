using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Properties;
using Brokerage.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Brokerage.API.Controllers;

// Manages property creation, media uploads, amenity assignments, seller management, and buyer listings.

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly PropertyService _propertyService;

    public PropertyController(PropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    /// <summary>
    /// Extracts authenticated User ID and Role from current HTTP Context Claims.
    /// </summary>
    /// <returns>Tuple containing nullable UserId and Role string.</returns>
    private (int? UserId, string? Role) GetCurrentUser()
    {
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        string? roleClaim = User.FindFirstValue(ClaimTypes.Role)
            ?? User.FindFirstValue("role");

        if (int.TryParse(userIdClaim, out int userId))
        {
            return (userId, roleClaim);
        }

        return (null, null);
    }

    /// <summary>
    /// Creates a new property listing in 'Draft' status for the authenticated seller.
    /// </summary>
    /// <remarks>
    /// Flow: Validates user identity -> Creates draft record -> Logs activity -> Returns generated Property ID.
    /// </remarks>
    /// <param name="request">Property creation parameters.</param>
    /// <returns>Result containing created Property ID.</returns>
    [Authorize(Roles = "Seller")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateProperty(CreatePropertyRequest request)
    {
        var (userId, _) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        CreatePropertyResponse response = await _propertyService.CreatePropertyAsync(request, userId.Value);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all properties belonging to the currently logged-in seller.
    /// </summary>
    /// <remarks>
    /// Flow: Identifies seller ID -> Queries repository for seller's properties -> Returns property list.
    /// </remarks>
    /// <returns>List of seller's owned property listings.</returns>
    [Authorize(Roles = "Seller")]
    [HttpGet("my-properties")]
    public async Task<IActionResult> GetMyProperties()
    {
        var (userId, _) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        IEnumerable<Property> properties = await _propertyService.GetMyPropertiesAsync(userId.Value);
        return Ok(properties);
    }

    /// <summary>
    /// Retrieves property details by Property ID based on caller authorization permissions.
    /// </summary>
    /// <remarks>
    /// Flow: Enforces authorization (Seller views own property, Buyer views Approved listing) -> Returns entity.
    /// </remarks>
    /// <param name="propertyId">Unique Property ID.</param>
    /// <returns>Property entity if authorized and found.</returns>
    [Authorize]
    [HttpGet("{propertyId:int}")]
    public async Task<IActionResult> GetPropertyById(int propertyId)
    {
        var (userId, role) = GetCurrentUser();
        if (!userId.HasValue || string.IsNullOrWhiteSpace(role)) return Unauthorized();

        Property? property = await _propertyService.GetPropertyByIdAsync(propertyId, userId.Value, role);

        if (property == null)
        {
            return NotFound(new { success = false, message = "Property not found." });
        }

        return Ok(property);
    }

    /// <summary>
    /// Retrieves property review details for a seller by Property ID.
    /// </summary>
    /// <remarks>
    /// Flow: Validates seller ownership -> Fetches property, amenities, and media records.
    /// </remarks>
    /// <param name="propertyId">Unique Property ID.</param>
    /// <returns>Full property review response payload.</returns>
    [HttpGet("{propertyId:int}/review")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetPropertyReviewById(
    int propertyId)
    {
        string? userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdClaim,
                out int sellerId))
        {
            return Unauthorized();
        }

        PropertyReviewResponse? review =
            await _propertyService
                .GetPropertyReviewAsync(
                    propertyId,
                    sellerId,
                    "Seller");

        if (review == null)
        {
            return NotFound(
                new
                {
                    success = false,
                    message = "Property not found."
                });
        }

        return Ok(review);
    }

    /// <summary>
    /// Retrieves property review details using Property GUID for authorized Sellers or Buyers.
    /// </summary>
    /// <remarks>
    /// Flow: Checks ownership/approved status -> Fetches combined property details, amenities, and media.
    /// </remarks>
    /// <param name="propertyGuid">Unique Property GUID.</param>
    /// <returns>Full property review model.</returns>
    [Authorize(Roles = "Seller,Buyer")]
    [HttpGet("{propertyGuid:guid}/review")]
    public async Task<IActionResult> GetPropertyReview(Guid propertyGuid)
    {
        var (userId, role) = GetCurrentUser();
        if (!userId.HasValue || string.IsNullOrWhiteSpace(role)) return Unauthorized();

        PropertyReviewResponse? review = await _propertyService.GetPropertyReviewAsync(propertyGuid, userId.Value, role);

        if (review == null)
        {
            return NotFound(new { success = false, message = "Property not found." });
        }

        return Ok(review);
    }

    /// <summary>
    /// Updates property details for an existing listing in Draft or Rejected status.
    /// </summary>
    /// <remarks>
    /// Flow: Verifies seller ownership &amp; Draft/Rejected state -> Updates property data -> Logs activity.
    /// </remarks>
    /// <param name="propertyId">Target Property ID.</param>
    /// <param name="request">Updated property fields payload.</param>
    /// <returns>Operation status result.</returns>
    [Authorize(Roles = "Seller")]
    [HttpPut("{propertyId:int}")]
    public async Task<IActionResult> UpdateProperty(int propertyId, UpdatePropertyRequest request)
    {
        var (userId, _) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        bool updated = await _propertyService.UpdatePropertyAsync(propertyId, userId.Value, request);

        if (!updated)
        {
            return NotFound(new { success = false, message = "Property not found or you are not allowed to edit it." });
        }

        return Ok(new { success = true, message = "Property updated successfully." });
    }

    /// <summary>
    /// Submits a draft or rejected property listing for administrator review.
    /// </summary>
    /// <remarks>
    /// Flow: Validates ownership -> Checks required details, amenities &amp; media -> Changes status to 'Pending' -> Logs activity.
    /// </remarks>
    /// <param name="propertyId">Target Property ID.</param>
    /// <returns>Property submission status response.</returns>
    [HttpPut("{propertyId:int}/submit")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> SubmitProperty(int propertyId)
    {
        var (userId, _) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        SubmitPropertyResponse response = await _propertyService.SubmitPropertyAsync(propertyId, userId.Value);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Retrieves a list of all available property amenities.
    /// </summary>
    /// <remarks>
    /// Flow: Queries repository for complete list of master amenities categorized by type.
    /// </remarks>
    /// <returns>List of amenity entities.</returns>
    [Authorize]
    [HttpGet("amenities")]
    public async Task<IActionResult> GetAllAmenities()
    {
        IEnumerable<Amenity> amenities = await _propertyService.GetAllAmenitiesAsync();
        return Ok(amenities);
    }

    /// <summary>
    /// Saves or updates selected amenity associations for a draft or rejected property listing.
    /// </summary>
    /// <remarks>
    /// Flow: Verifies seller ownership &amp; editable status -> Replaces existing amenities -> Logs activity.
    /// </remarks>
    /// <param name="propertyId">Target Property ID.</param>
    /// <param name="request">Payload containing list of selected Amenity IDs.</param>
    /// <returns>Operation result message.</returns>
    [Authorize(Roles = "Seller")]
    [HttpPost("{propertyId:int}/amenities")]
    public async Task<IActionResult> SavePropertyAmenities(int propertyId, AddPropertyAmenitiesRequest request)
    {
        var (userId, _) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        bool saved = await _propertyService.SavePropertyAmenitiesAsync(propertyId, userId.Value, request.AmenityIDs);

        if (!saved)
        {
            return BadRequest(new { success = false, message = "Amenities could not be saved." });
        }

        return Ok(new { success = true, message = "Property amenities saved successfully." });
    }

    /// <summary>
    /// Uploads media files (cover photo, gallery images, videos, floor plans, documents) for a property.
    /// </summary>
    /// <remarks>
    /// Flow: Validates ownership &amp; editable state -> Validates file limits/formats -> Saves to disk -> Persists metadata -> Logs activity.
    /// </remarks>
    /// <param name="propertyId">Target Property ID.</param>
    /// <param name="request">Multipart form data containing media files.</param>
    /// <returns>Upload operation status.</returns>
    [HttpPost("{propertyId:int}/media")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UploadMedia(int propertyId, [FromForm] MediaUploadRequest request)
    {
        var (userId, _) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        try
        {
            bool result = await _propertyService.UploadPropertyMediaAsync(propertyId, userId.Value, request);

            if (!result)
            {
                return BadRequest(new { success = false, message = "Media upload failed." });
            }

            return Ok(new { success = true, message = "Property media uploaded successfully." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Retrieves uploaded media files associated with a specific property.
    /// </summary>
    /// <remarks>
    /// Flow: Enforces authorization rules -> Fetches media list sorted by display order.
    /// </remarks>
    /// <param name="propertyId">Target Property ID.</param>
    /// <returns>Collection of property media records.</returns>
    [HttpGet("{propertyId:int}/media")]
    [Authorize]
    public async Task<IActionResult> GetPropertyMedia(int propertyId)
    {
        var (userId, role) = GetCurrentUser();
        if (!userId.HasValue) return Unauthorized();

        IEnumerable<PropertyMedia> media = await _propertyService.GetPropertyMediaAsync(propertyId, userId.Value, role ?? string.Empty);
        return Ok(media);
    }

    /// <summary>
    /// Searches approved property listings using buyer search filters and pagination parameters.
    /// </summary>
    /// <remarks>
    /// Flow: Validates price/location/bedroom filters -> Normalizes sorting -> Queries approved listings -> Returns property cards with amenities &amp; media.
    /// </remarks>
    /// <param name="request">Search parameters, filters, location type, and pagination information.</param>
    /// <param name="cancellationToken">Cancellation token for asynchronous operation.</param>
    /// <returns>Paginated list of approved buyer property listings.</returns>
    [Authorize(Roles = "Buyer")]
    [HttpGet("buyer-listings")]
    public async Task<ActionResult<BuyerPropertyListResponse>> GetBuyerListings(
        [FromQuery] BuyerPropertySearchRequest request,
        CancellationToken cancellationToken)
    {
        var (buyerId, _) = GetCurrentUser();
        if (!buyerId.HasValue) return Unauthorized();

        if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
        {
            return BadRequest("Minimum price cannot be greater than maximum price.");
        }

        if (!string.IsNullOrWhiteSpace(request.LocationType))
        {
            bool validLocationType = request.LocationType.Equals("State", StringComparison.OrdinalIgnoreCase)
                || request.LocationType.Equals("City", StringComparison.OrdinalIgnoreCase);

            if (!validLocationType) return BadRequest("Location type must be State or City.");
            if (string.IsNullOrWhiteSpace(request.LocationValue)) return BadRequest("Location value is required.");
        }

        if (request.MinBedrooms.HasValue && request.MinBedrooms <= 0)
        {
            return BadRequest("Minimum bedrooms must be greater than zero.");
        }

        string sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? "latest";
        if (sortBy != "latest" && sortBy != "price-low" && sortBy != "price-high")
        {
            return BadRequest("Invalid sort option.");
        }

        request.SortBy = sortBy;

        BuyerPropertyListResponse result = await _propertyService.SearchApprovedPropertiesAsync(
            request, buyerId.Value, cancellationToken);

        return Ok(result);
    }
}