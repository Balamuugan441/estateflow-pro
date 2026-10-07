using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Properties;
using Brokerage.Models.Entities;
using Microsoft.AspNetCore.Http;


namespace Brokerage.Business.Services;

public class PropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMediaValidationService _mediaValidationService;
    private readonly ActivityLogService _activityLogService;

    public PropertyService(IPropertyRepository propertyRepository, IFileStorageService fileStorageService, IMediaValidationService mediaValidationService, ActivityLogService activityLogService)
    {
        _propertyRepository = propertyRepository;
        _fileStorageService = fileStorageService;
        _mediaValidationService = mediaValidationService;
        _activityLogService = activityLogService;
    }

    public async Task<CreatePropertyResponse> CreatePropertyAsync(
        CreatePropertyRequest request,
        int sellerId)
    {
        Property property = new Property
        {
            SellerID = sellerId,

            PropertyTitle = request.PropertyTitle,
            PropertyType = request.PropertyType,
            ListingType = request.ListingType,
            PropertyStatus = request.PropertyStatus,

            ListingStatus = "Draft",

            LocationAddress = request.LocationAddress,
            Country = request.Country,
            State = request.State,
            City = request.City,
            ZipCode = request.ZipCode,

            Price = request.Price,
            SecurityDeposit = request.SecurityDeposit,

            Area = request.Area,
            AreaUnit = request.AreaUnit,

            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,

            Balconies = request.Balconies,
            Floor = request.Floor,
            ParkingSpaces = request.ParkingSpaces,

            YearBuilt = request.YearBuilt,
            PropertyAgeYears = request.PropertyAgeYears,
            PossessionDate = request.PossessionDate,

            FurnishingType = request.FurnishingType,
            FacingDirection = request.FacingDirection,

            PreferredTenants = request.PreferredTenants,
            TenantFoodPreference = request.TenantFoodPreference,

            Description = request.Description
        };

        int propertyId =
     await _propertyRepository.CreatePropertyAsync(property);

        await _activityLogService.LogCurrentUserAsync(
            "PROPERTY_CREATED",
            "PROPERTY",
            "PROPERTY",
            propertyId,
            property.PropertyTitle,
            "Success",
            "Property was created as Draft.");

        return new CreatePropertyResponse
        {
            Success = true,
            Message = "Property created successfully.",
            PropertyID = propertyId
        };
    }
    public async Task<IEnumerable<Property>> GetMyPropertiesAsync(
    int sellerId)
    {
        return await _propertyRepository
            .GetPropertiesBySellerAsync(sellerId);
    }
    public async Task<Property?> GetPropertyByIdAsync(int propertyId, int userId, string role)
    {
        Property? property = await _propertyRepository.GetPropertyByIdAsync(propertyId);
        if (property == null) return null;

        if (string.Equals(role, "Seller", StringComparison.OrdinalIgnoreCase))
        {
            return property.SellerID == userId ? property : null;
        }

        if (string.Equals(role, "Buyer", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(property.ListingStatus, "Approved", StringComparison.OrdinalIgnoreCase) ? property : null;
        }

        return null;
    }
    public async Task<bool> UpdatePropertyAsync(
    int propertyId,
    int sellerId,
    UpdatePropertyRequest request)
    {
        Property? existingProperty =
            await _propertyRepository.GetPropertyByIdAsync(
                propertyId);

        if (existingProperty == null)
        {
            return false;
        }

        // Check ownership
        if (existingProperty.SellerID != sellerId)
        {
            return false;
        }

        // Only Draft and Rejected properties can be edited
        if (existingProperty.ListingStatus != "Draft" &&
            existingProperty.ListingStatus != "Rejected")
        {
            return false;
        }

        existingProperty.PropertyTitle =
            request.PropertyTitle;

        existingProperty.PropertyType =
            request.PropertyType;

        existingProperty.ListingType =
            request.ListingType;

        existingProperty.PropertyStatus =
            request.PropertyStatus;

        existingProperty.LocationAddress =
            request.LocationAddress;

        existingProperty.Country =
            request.Country;

        existingProperty.State =
            request.State;

        existingProperty.City =
            request.City;

        existingProperty.ZipCode =
            request.ZipCode;

        existingProperty.Price =
            request.Price;

        existingProperty.SecurityDeposit =
            request.SecurityDeposit;

        existingProperty.Area =
            request.Area;

        existingProperty.AreaUnit =
            request.AreaUnit;

        existingProperty.Bedrooms =
            request.Bedrooms;

        existingProperty.Bathrooms =
            request.Bathrooms;

        existingProperty.Balconies =
            request.Balconies;

        existingProperty.Floor =
            request.Floor;

        existingProperty.ParkingSpaces =
            request.ParkingSpaces;

        existingProperty.YearBuilt =
            request.YearBuilt;

        existingProperty.PropertyAgeYears =
            request.PropertyAgeYears;

        existingProperty.PossessionDate =
            request.PossessionDate;

        existingProperty.FurnishingType =
            request.FurnishingType;

        existingProperty.FacingDirection =
            request.FacingDirection;

        existingProperty.PreferredTenants =
            request.PreferredTenants;

        existingProperty.TenantFoodPreference =
            request.TenantFoodPreference;

        existingProperty.Description =
            request.Description;

        bool updated =
    await _propertyRepository
        .UpdatePropertyAsync(existingProperty);

        if (updated)
        {
            await _activityLogService.LogCurrentUserAsync(
                "PROPERTY_UPDATED",
                "PROPERTY",
                "PROPERTY",
                propertyId,
                request.PropertyTitle,
                "Success",
                "Property details were updated.");
        }

        return updated;
    }
    public async Task<SubmitPropertyResponse>
     SubmitPropertyAsync(
         int propertyId,
         int sellerId)
    {
        Property? property =
            await _propertyRepository.GetPropertyByIdAsync(
                propertyId);

        if (property == null)
        {
            return new SubmitPropertyResponse
            {
                Success = false,
                Message = "Property not found.",
                Errors =
                [
                    "The requested property does not exist."
                ]
            };
        }

        // Check property ownership.
        if (property.SellerID != sellerId)
        {
            return new SubmitPropertyResponse
            {
                Success = false,
                Message = "You are not allowed to submit this property.",
                Errors =
                [
                    "This property does not belong to you."
                ]
            };
        }
        if (property.ListingStatus == "Pending")
        {
            return new SubmitPropertyResponse
            {
                Success = true,

                Message =
                    "Property has already been submitted and is pending admin approval."
            };
        }


        // Only Draft and Rejected properties can be submitted.
        if (property.ListingStatus != "Draft" &&
            property.ListingStatus != "Rejected")
        {
            return new SubmitPropertyResponse
            {
                Success = false,
                Message = "Property cannot be submitted.",
                Errors =
                [
                    $"A property with status '{property.ListingStatus}' cannot be submitted."
                ]
            };
        }

        List<string> errors = [];

        //Property Validation

        if (string.IsNullOrWhiteSpace(property.PropertyTitle))
        {
            errors.Add("Property title is required.");
        }

        if (string.IsNullOrWhiteSpace(property.PropertyType))
        {
            errors.Add("Property type is required.");
        }

        if (string.IsNullOrWhiteSpace(property.ListingType))
        {
            errors.Add("Listing type is required.");
        }

        if (string.IsNullOrWhiteSpace(property.PropertyStatus))
        {
            errors.Add("Property status is required.");
        }

        if (string.IsNullOrWhiteSpace(property.LocationAddress))
        {
            errors.Add("Location address is required.");
        }

        if (string.IsNullOrWhiteSpace(property.Country))
        {
            errors.Add("Country is required.");
        }

        if (string.IsNullOrWhiteSpace(property.State))
        {
            errors.Add("State is required.");
        }

        if (string.IsNullOrWhiteSpace(property.City))
        {
            errors.Add("City is required.");
        }

        if (string.IsNullOrWhiteSpace(property.ZipCode))
        {
            errors.Add("Zip code is required.");
        }

        if (property.Price <= 0)
        {
            errors.Add("Price must be greater than zero.");
        }

        if (property.Area <= 0)
        {
            errors.Add("Area must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(property.AreaUnit))
        {
            errors.Add("Area unit is required.");
        }

        if (string.IsNullOrWhiteSpace(property.Bedrooms))
        {
            errors.Add("Bedrooms is required.");
        }

        if (property.Bathrooms < 0)
        {
            errors.Add("Bathrooms cannot be negative.");
        }


        // Validate Amenities


        IEnumerable<Amenity> amenities =
            await _propertyRepository
                .GetPropertyAmenitiesAsync(propertyId);

        if (!amenities.Any())
        {
            errors.Add(
                "At least one amenity must be selected.");
        }

        // --------------------------------
        // Validate Media
        // --------------------------------

        IEnumerable<PropertyMedia> media =
            await _propertyRepository
                .GetPropertyMediaAsync(propertyId);

        bool hasCoverPhoto =
            media.Any(x =>
                x.MediaType == "CoverPhoto");

        bool hasGalleryImage =
            media.Any(x =>
                x.MediaType == "GalleryImage");

        if (!hasCoverPhoto)
        {
            errors.Add(
                "Cover photo is required.");
        }

        if (!hasGalleryImage)
        {
            errors.Add(
                "At least one gallery image is required.");
        }


        // Stop if validation failed


        if (errors.Count > 0)
        {
            return new SubmitPropertyResponse
            {
                Success = false,
                Message = "Property cannot be submitted.",
                Errors = errors
            };
        }

        // Everything is valid


        string previousStatus =
    property.ListingStatus;

        bool submitted =
            await _propertyRepository
                .UpdateListingStatusAsync(
                    propertyId,
                    "Pending");

        if (!submitted)
        {
            return new SubmitPropertyResponse
            {
                Success = false,
                Message = "Property submission failed.",
                Errors =
                [
                    "Unable to change the property status to Pending."
                ]
            };
        }

        await _activityLogService.LogCurrentUserAsync(
            "PROPERTY_SUBMITTED",
            "PROPERTY",
            "PROPERTY",
            propertyId,
            property.PropertyTitle,
            "Success",
            $"{previousStatus} → Pending. Property submitted for admin approval.");

        return new SubmitPropertyResponse
        {
            Success = true,
            Message =
                "Property submitted successfully and is now pending admin approval."
        };
    }
    public async Task<IEnumerable<Amenity>> GetAllAmenitiesAsync()
    {
        return await _propertyRepository
            .GetAllAmenitiesAsync();
    }
    public async Task<bool> SavePropertyAmenitiesAsync(
    int propertyId,
    int sellerId,
    IEnumerable<int> amenityIds)
    {
        Property? property =
            await _propertyRepository.GetPropertyByIdAsync(
                propertyId);

        if (property == null)
        {
            return false;
        }

        // Make sure the property belongs to the logged-in seller.
        if (property.SellerID != sellerId)
        {
            return false;
        }

        // Only Draft or Rejected properties can be modified.
        if (property.ListingStatus != "Draft" &&
            property.ListingStatus != "Rejected")
        {
            return false;
        }

        bool saved =
    await _propertyRepository
        .SavePropertyAmenitiesAsync(
            propertyId,
            amenityIds);

        if (saved)
        {
            await _activityLogService.LogCurrentUserAsync(
                "PROPERTY_AMENITIES_UPDATED",
                "PROPERTY",
                "PROPERTY",
                propertyId,
                property.PropertyTitle,
                "Success",
                "Property amenities were updated.");
        }

        return saved;
    }
    public async Task<bool> UploadPropertyMediaAsync(
    int propertyId,
    int sellerId,
    MediaUploadRequest request)
    {
        Property? property =
            await _propertyRepository.GetPropertyByIdAsync(
                propertyId);

        if (property == null)
        {
            throw new Exception("Property not found.");
        }

        if (property.SellerID != sellerId)
        {
            throw new UnauthorizedAccessException(
                "You do not own this property.");
        }

        if (property.ListingStatus != "Draft" &&
            property.ListingStatus != "Rejected")
        {
            throw new InvalidOperationException(
                "Media can only be uploaded for Draft or Rejected properties.");
        }

        if (request.CoverPhoto == null)
        {
            throw new InvalidOperationException(
                "Cover photo is required.");
        }

        if (request.GalleryImages == null ||
            request.GalleryImages.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one gallery image is required.");
        }
        if (request.GalleryImages.Count > 7)
        {
            throw new InvalidOperationException(
                "Maximum 7 gallery images are allowed.");
        }

        if (request.Videos.Count > 2)
        {
            throw new InvalidOperationException(
                "Maximum 2 videos are allowed.");
        }

        if (request.FloorPlans.Count > 2)
        {
            throw new InvalidOperationException(
                "Maximum 2 floor plans are allowed.");
        }

        if (request.Documents.Count > 4)
        {
            throw new InvalidOperationException(
                "Maximum 4 documents are allowed.");
        }

        List<PropertyMedia> mediaList = [];

        const long FiveMB = 5 * 1024 * 1024;

        const long TwentyFiveMB = 25 * 1024 * 1024;

        // Cover Photo

        string? coverError =
                _mediaValidationService.ValidateImage(
                    request.CoverPhoto,
                    FiveMB);

        if (coverError != null)
        {
            throw new InvalidOperationException(
                coverError);
        }

        string coverPath =
            await _fileStorageService.SaveFileAsync(
                request.CoverPhoto.OpenReadStream(),
                request.CoverPhoto.FileName,
                $"properties/{propertyId}");

        mediaList.Add(new PropertyMedia
        {
            PropertyID = propertyId,
            MediaType = "CoverPhoto",
            FileName = request.CoverPhoto.FileName,
            FilePath = coverPath,
            ContentType = request.CoverPhoto.ContentType,
            FileSizeBytes = request.CoverPhoto.Length,
            DisplayOrder = 1
        });

        // Gallery Image

        int displayOrder = 2;

        foreach (IFormFile galleryImage
                 in request.GalleryImages)
        {
            string? galleryError =
                _mediaValidationService.ValidateImage(
                    galleryImage,
                    FiveMB);

            if (galleryError != null)
            {
                throw new InvalidOperationException(
                    galleryError);
            }

            string galleryPath =
                await _fileStorageService.SaveFileAsync(
                    galleryImage.OpenReadStream(),
                    galleryImage.FileName,
                    $"properties/{propertyId}");

            mediaList.Add(new PropertyMedia
            {
                PropertyID = propertyId,
                MediaType = "GalleryImage",
                FileName = galleryImage.FileName,
                FilePath = galleryPath,
                ContentType = galleryImage.ContentType,
                FileSizeBytes = galleryImage.Length,
                DisplayOrder = displayOrder++
            });
        }

        // Video


        foreach (IFormFile video in request.Videos)
        {
            string? videoError =
                _mediaValidationService.ValidateVideo(
                    video,
                    TwentyFiveMB);

            if (videoError != null)
            {
                throw new InvalidOperationException(
                    videoError);
            }

            string videoPath =
                await _fileStorageService.SaveFileAsync(
                    video.OpenReadStream(),
                    video.FileName,
                    $"properties/{propertyId}");

            mediaList.Add(new PropertyMedia
            {
                PropertyID = propertyId,
                MediaType = "Video",
                FileName = video.FileName,
                FilePath = videoPath,
                ContentType = video.ContentType,
                FileSizeBytes = video.Length,
                DisplayOrder = null
            });
        }


        // Floor Plan

        foreach (IFormFile floorPlan in request.FloorPlans)
        {
            string? floorPlanError =
                _mediaValidationService.ValidateDocument(
                    floorPlan,
                    FiveMB);

            if (floorPlanError != null)
            {
                throw new InvalidOperationException(
                    floorPlanError);
            }

            string floorPlanPath =
                await _fileStorageService.SaveFileAsync(
                    floorPlan.OpenReadStream(),
                    floorPlan.FileName,
                    $"properties/{propertyId}");

            mediaList.Add(new PropertyMedia
            {
                PropertyID = propertyId,
                MediaType = "FloorPlan",
                FileName = floorPlan.FileName,
                FilePath = floorPlanPath,
                ContentType = floorPlan.ContentType,
                FileSizeBytes = floorPlan.Length,
                DisplayOrder = null
            });
        }
        // Documents


        foreach (IFormFile document
          in request.Documents)
        {
            string? documentError =
                _mediaValidationService.ValidateDocument(
                    document,
                    FiveMB);

            if (documentError != null)
            {
                throw new InvalidOperationException(
                    documentError);
            }

            string documentPath =
                await _fileStorageService.SaveFileAsync(
                    document.OpenReadStream(),
                    document.FileName,
                    $"properties/{propertyId}");

            mediaList.Add(new PropertyMedia
            {
                PropertyID = propertyId,
                MediaType = "Document",
                FileName = document.FileName,
                FilePath = documentPath,
                ContentType = document.ContentType,
                FileSizeBytes = document.Length,
                DisplayOrder = null
            });
        }

        bool saved =
     await _propertyRepository
         .SavePropertyMediaAsync(
             mediaList);

        if (saved)
        {
            await _activityLogService.LogCurrentUserAsync(
                "PROPERTY_MEDIA_UPDATED",
                "PROPERTY",
                "PROPERTY",
                propertyId,
                property.PropertyTitle,
                "Success",
                "Property media was updated.");
        }

        return saved;
    }
    public async Task<IEnumerable<PropertyMedia>> GetPropertyMediaAsync(
    int propertyId,
    int userId,
    string role)
    {
        Property? property =
            await _propertyRepository.GetPropertyByIdAsync(
                propertyId);

        if (property == null)
        {
            return [];
        }

        // Seller can view media only for their own property.
        if (role == "Seller")
        {
            if (property.SellerID != userId)
            {
                return [];
            }
        }

        // Buyer can view media only for approved properties.
        else if (role == "Buyer")
        {
            if (property.ListingStatus != "Approved")
            {
                return [];
            }
        }

        else
        {
            return [];
        }

        return await _propertyRepository
            .GetPropertyMediaAsync(propertyId);
    }
    public async Task<PropertyReviewResponse?>
    GetPropertyReviewAsync(
        int propertyId,
        int userId,
        string role)
    {
        Property? property =
            await _propertyRepository
                .GetPropertyByIdAsync(
                    propertyId);

        if (property == null)
        {
            return null;
        }

        if (string.Equals(
                role,
                "Seller",
                StringComparison.OrdinalIgnoreCase))
        {
            if (property.SellerID != userId)
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        IEnumerable<Amenity> amenities =
            await _propertyRepository
                .GetPropertyAmenitiesAsync(
                    property.PropertyID);

        IEnumerable<PropertyMedia> media =
            await _propertyRepository
                .GetPropertyMediaAsync(
                    property.PropertyID);

        return new PropertyReviewResponse
        {
            Property = property,
            Amenities = amenities,
            Media = media
        };
    }
    public async Task<PropertyReviewResponse?> GetPropertyReviewAsync(Guid propertyGuid, int userId, string role)
    {
        Property? property = await _propertyRepository.GetPropertyByGuidAsync(propertyGuid);
        if (property == null) return null;

        if (string.Equals(role, "Seller", StringComparison.OrdinalIgnoreCase))
        {
            if (property.SellerID != userId) return null;
        }
        else if (string.Equals(role, "Buyer", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(property.ListingStatus, "Approved", StringComparison.OrdinalIgnoreCase)) return null;
        }
        else
        {
            return null;
        }

        IEnumerable<Amenity> amenities = await _propertyRepository.GetPropertyAmenitiesAsync(property.PropertyID);
        IEnumerable<PropertyMedia> media = await _propertyRepository.GetPropertyMediaAsync(property.PropertyID);

        return new PropertyReviewResponse
        {
            Property = property,
            Amenities = amenities,
            Media = media
        };
    }
    /// <summary>
    /// Searches approved property listings using Buyer search criteria.
    /// </summary>
    /// <remarks>
    /// Called by PropertyController in the API layer.
    /// Delegates database retrieval to <see cref="IPropertyRepository"/>.
    ///
    /// The service normalizes user-supplied search values before
    /// passing them to the repository.
    /// </remarks>
    public async Task<BuyerPropertyListResponse>
    SearchApprovedPropertiesAsync(
        BuyerPropertySearchRequest request,
        int buyerId,
        CancellationToken cancellationToken = default)
    {
        request.PageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        request.Search =
            string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();

        request.LocationType =
            string.IsNullOrWhiteSpace(request.LocationType)
                ? null
                : request.LocationType.Trim();

        request.LocationValue =
            string.IsNullOrWhiteSpace(request.LocationValue)
                ? null
                : request.LocationValue.Trim();

        request.PropertyType =
            string.IsNullOrWhiteSpace(request.PropertyType)
                ? null
                : request.PropertyType.Trim();

        request.SortBy =
            string.IsNullOrWhiteSpace(request.SortBy)
                ? "latest"
                : request.SortBy.Trim().ToLowerInvariant();

        request.ListingTypes =
            request.ListingTypes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

        return await _propertyRepository
            .SearchApprovedPropertiesAsync(
                request,
                buyerId,
                cancellationToken);
    }


    public async Task<PropertyReviewResponse?> GetPropertyReviewByIdAsync(int propertyId, int userId, string role)
    {
        Property? property = await _propertyRepository.GetPropertyByIdAsync(propertyId);
        if (property == null) return null;

        if (string.Equals(role, "Seller", StringComparison.OrdinalIgnoreCase))
        {
            if (property.SellerID != userId) return null;
        }
        else if (string.Equals(role, "Buyer", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(property.ListingStatus, "Approved", StringComparison.OrdinalIgnoreCase)) return null;
        }
        else
        {
            return null;
        }

        IEnumerable<Amenity> amenities = await _propertyRepository.GetPropertyAmenitiesAsync(property.PropertyID);
        IEnumerable<PropertyMedia> media = await _propertyRepository.GetPropertyMediaAsync(property.PropertyID);

        return new PropertyReviewResponse
        {
            Property = property,
            Amenities = amenities,
            Media = media
        };
    }




}