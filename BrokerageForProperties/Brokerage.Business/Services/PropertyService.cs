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
            Bathrooms = request.Bathrooms > 0 ? request.Bathrooms : 2m,
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
            request.Bathrooms > 0
        ? request.Bathrooms
        : 2m;

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
            await _propertyRepository
                .GetPropertyByIdAsync(propertyId);

        if (property == null)
        {
            throw new Exception(
                "Property not found."
            );
        }


        if (property.SellerID != sellerId)
        {
            throw new UnauthorizedAccessException(
                "You do not own this property."
            );
        }



        if (property.ListingStatus != "Draft" &&
            property.ListingStatus != "Rejected")
        {
            throw new InvalidOperationException(
                "Media can only be uploaded for Draft or Rejected properties."
            );
        }


        IFormFile? coverPhoto =
            IsRealFile(request.CoverPhoto)
                ? request.CoverPhoto
                : null;


        List<IFormFile> galleryImages =
            GetRealFiles(
                request.GalleryImages
            );


        List<IFormFile> videos =
            GetRealFiles(
                request.Videos
            );


        List<IFormFile> floorPlans =
            GetRealFiles(
                request.FloorPlans
            );


        List<IFormFile> documents =
            GetRealFiles(
                request.Documents
            );


        IEnumerable<PropertyMedia> existingMedia =
            await _propertyRepository
                .GetPropertyMediaAsync(
                    propertyId
                );
        foreach (int mediaId in request.RemovedMediaIds.Distinct())
        {
            PropertyMedia? media =
                existingMedia.FirstOrDefault(
                    x => x.MediaID == mediaId);

            if (media == null)
            {
                throw new InvalidOperationException(
                    "Media was not found.");
            }

            if (media.MediaType == "CoverPhoto")
            {
                throw new InvalidOperationException(
                    "Cover photo cannot be removed.");
            }

            bool deleted =
                await _propertyRepository
                    .DeletePropertyMediaAsync(
                        propertyId,
                        mediaId);

            if (!deleted)
            {
                throw new InvalidOperationException(
                    "Unable to remove media.");
            }
        }
        PropertyMedia? existingCover =
    existingMedia.FirstOrDefault(
        x => x.MediaType == "CoverPhoto");

        int existingCoverCount =
            existingMedia.Count(
                x => x.MediaType == "CoverPhoto"
            );


        int existingGalleryCount =
            existingMedia.Count(
                x => x.MediaType == "GalleryImage"
            );


        int existingVideoCount =
            existingMedia.Count(
                x => x.MediaType == "Video"
            );


        int existingFloorPlanCount =
            existingMedia.Count(
                x => x.MediaType == "FloorPlan"
            );


        int existingDocumentCount =
            existingMedia.Count(
                x => x.MediaType == "Document"
            );


        // -----------------------------------------
        // Cover validation
        //
        // Existing cover is enough.
        // -----------------------------------------

        if (coverPhoto == null &&
            existingCoverCount == 0)
        {
            throw new InvalidOperationException(
                "Cover photo is required."
            );
        }


        // -----------------------------------------
        // Gallery validation
        //
        // Existing gallery is enough.
        // -----------------------------------------

        if (galleryImages.Count == 0 &&
            existingGalleryCount == 0)
        {
            throw new InvalidOperationException(
                "At least one gallery image is required."
            );
        }


        // -----------------------------------------
        // Total gallery limit
        // -----------------------------------------

        if (
            existingGalleryCount +
            galleryImages.Count > 7
        )
        {
            throw new InvalidOperationException(
                "Maximum 7 gallery images are allowed in total."
            );
        }


        // -----------------------------------------
        // Total video limit
        // -----------------------------------------

        if (
            existingVideoCount +
            videos.Count > 2
        )
        {
            throw new InvalidOperationException(
                "Maximum 2 videos are allowed in total."
            );
        }


        // -----------------------------------------
        // Total floor-plan limit
        // -----------------------------------------

        if (
            existingFloorPlanCount +
            floorPlans.Count > 2
        )
        {
            throw new InvalidOperationException(
                "Maximum 2 floor plans are allowed in total."
            );
        }


        // -----------------------------------------
        // Total document limit
        // -----------------------------------------

        if (
            existingDocumentCount +
            documents.Count > 4
        )
        {
            throw new InvalidOperationException(
                "Maximum 4 documents are allowed in total."
            );
        }


        List<PropertyMedia> mediaList = [];


        const long FiveMB =
            5 * 1024 * 1024;


        const long TwentyFiveMB =
            25 * 1024 * 1024;

        // COVER PHOTO


        if (request.CoverPhoto != null)
        {
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
                    $"properties/{propertyId}"
                );


            PropertyMedia newCover =
                new PropertyMedia
                {
                    PropertyID = propertyId,
                    MediaType = "CoverPhoto",
                    FileName = request.CoverPhoto.FileName,
                    FilePath = coverPath,
                    ContentType = request.CoverPhoto.ContentType,
                    FileSizeBytes = request.CoverPhoto.Length,
                    DisplayOrder = 1
                };


            // Existing cover → UPDATE


            if (existingCover != null)
            {
                newCover.MediaID =
                    existingCover.MediaID;


                bool updated =
                    await _propertyRepository
                        .UpdatePropertyCoverMediaAsync(
                            newCover);


                if (!updated)
                {
                    throw new InvalidOperationException(
                        "Unable to update the existing cover photo."
                    );
                }
            }
            else
            {

                // No existing cover → INSERT
                mediaList.Add(newCover);
            }
        }


        // =========================================
        // GALLERY
        // =========================================

        int displayOrder = 2;

        for (int i = 0; i < request.GalleryImages.Count; i++)
        {
            IFormFile galleryImage =
                request.GalleryImages[i];

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

            int replacementId =
                i < request.GalleryReplacementIds.Count
                    ? request.GalleryReplacementIds[i]
                    : 0;

            if (replacementId > 0)
            {
                PropertyMedia? existing =
                    existingMedia.FirstOrDefault(
                        x =>
                            x.MediaID == replacementId &&
                            x.MediaType == "GalleryImage");

                if (existing == null)
                {
                    throw new InvalidOperationException(
                        "Gallery image to replace was not found.");
                }

                existing.FileName =
                    galleryImage.FileName;

                existing.FilePath =
                    galleryPath;

                existing.ContentType =
                    galleryImage.ContentType;

                existing.FileSizeBytes =
                    galleryImage.Length;

                bool updated =
                    await _propertyRepository
                        .UpdatePropertyMediaAsync(
                            existing);

                if (!updated)
                {
                    throw new InvalidOperationException(
                        "Unable to update gallery image.");
                }
            }
            else
            {
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
        }


        for (int i = 0; i < request.Videos.Count; i++)
        {
            IFormFile video =
                request.Videos[i];

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

            int replacementId =
                i < request.VideoReplacementIds.Count
                    ? request.VideoReplacementIds[i]
                    : 0;

            if (replacementId > 0)
            {
                PropertyMedia? existing =
                    existingMedia.FirstOrDefault(
                        x =>
                            x.MediaID == replacementId &&
                            x.MediaType == "Video");

                if (existing == null)
                {
                    throw new InvalidOperationException(
                        "Video to replace was not found.");
                }

                existing.FileName =
                    video.FileName;

                existing.FilePath =
                    videoPath;

                existing.ContentType =
                    video.ContentType;

                existing.FileSizeBytes =
                    video.Length;

                bool updated =
                    await _propertyRepository
                        .UpdatePropertyMediaAsync(
                            existing);

                if (!updated)
                {
                    throw new InvalidOperationException(
                        "Unable to update video.");
                }
            }
            else
            {
                mediaList.Add(new PropertyMedia
                {
                    PropertyID = propertyId,
                    MediaType = "Video",
                    FileName = video.FileName,
                    FilePath = videoPath,
                    ContentType = video.ContentType,
                    FileSizeBytes = video.Length
                });
            }
        }


        // =========================================
        // FLOOR PLANS
        // =========================================

        for (int i = 0; i < request.FloorPlans.Count; i++)
        {
            IFormFile floorPlan =
                request.FloorPlans[i];

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

            int replacementId =
                i < request.FloorPlanReplacementIds.Count
                    ? request.FloorPlanReplacementIds[i]
                    : 0;

            if (replacementId > 0)
            {
                PropertyMedia? existing =
                    existingMedia.FirstOrDefault(
                        x =>
                            x.MediaID == replacementId &&
                            x.MediaType == "FloorPlan");

                if (existing == null)
                {
                    throw new InvalidOperationException(
                        "Floor plan to replace was not found.");
                }

                existing.FileName =
                    floorPlan.FileName;

                existing.FilePath =
                    floorPlanPath;

                existing.ContentType =
                    floorPlan.ContentType;

                existing.FileSizeBytes =
                    floorPlan.Length;

                bool updated =
                    await _propertyRepository
                        .UpdatePropertyMediaAsync(
                            existing);

                if (!updated)
                {
                    throw new InvalidOperationException(
                        "Unable to update floor plan.");
                }
            }
            else
            {
                mediaList.Add(new PropertyMedia
                {
                    PropertyID = propertyId,
                    MediaType = "FloorPlan",
                    FileName = floorPlan.FileName,
                    FilePath = floorPlanPath,
                    ContentType = floorPlan.ContentType,
                    FileSizeBytes = floorPlan.Length
                });
            }
        }


        for (int i = 0; i < request.Documents.Count; i++)
        {
            IFormFile document =
                request.Documents[i];

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

            int replacementId =
                i < request.DocumentReplacementIds.Count
                    ? request.DocumentReplacementIds[i]
                    : 0;

            if (replacementId > 0)
            {
                PropertyMedia? existing =
                    existingMedia.FirstOrDefault(
                        x =>
                            x.MediaID == replacementId &&
                            x.MediaType == "Document");

                if (existing == null)
                {
                    throw new InvalidOperationException(
                        "Document to replace was not found.");
                }

                existing.FileName =
                    document.FileName;

                existing.FilePath =
                    documentPath;

                existing.ContentType =
                    document.ContentType;

                existing.FileSizeBytes =
                    document.Length;

                bool updated =
                    await _propertyRepository
                        .UpdatePropertyMediaAsync(
                            existing);

                if (!updated)
                {
                    throw new InvalidOperationException(
                        "Unable to update document.");
                }
            }
            else
            {
                mediaList.Add(new PropertyMedia
                {
                    PropertyID = propertyId,
                    MediaType = "Document",
                    FileName = document.FileName,
                    FilePath = documentPath,
                    ContentType = document.ContentType,
                    FileSizeBytes = document.Length
                });
            }
        }


        // =========================================
        // Nothing new to save
        //
        // Existing DB media already satisfies
        // the property requirements.
        // =========================================

        if (mediaList.Count == 0)
        {
            return true;
        }


        bool saved = true;

        if (mediaList.Count > 0)
        {
            saved =
                await _propertyRepository
                    .SavePropertyMediaAsync(
                        mediaList);
        }


        if (saved)
        {
            await _activityLogService.LogCurrentUserAsync(
                "PROPERTY_MEDIA_UPDATED",
                "PROPERTY",
                "PROPERTY",
                propertyId,
                property.PropertyTitle,
                "Success",
                "Property media was updated."
            );
        }


        return saved;
    }
    private static bool IsRealFile(IFormFile? file)
    {
        return file != null &&
               file.Length > 0 &&
               !string.IsNullOrWhiteSpace(file.FileName);
    }


    private static List<IFormFile> GetRealFiles(
        IEnumerable<IFormFile>? files)
    {
        return files?
            .Where(IsRealFile)
            .ToList()
            ?? [];
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