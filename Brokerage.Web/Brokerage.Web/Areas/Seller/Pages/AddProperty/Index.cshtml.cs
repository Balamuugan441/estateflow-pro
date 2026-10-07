using Brokerage.Web.Models;
using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiRequests;
using Brokerage.Web.Models.ApiResponses;
using Brokerage.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Brokerage.Web.Areas.Seller.Pages.AddProperty;

public class AmenityOption
{
    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? IconPath { get; set; }
}

public class IndexModel : PageModel
{
    private readonly PropertyApiClient _propertyApiClient;


    public IndexModel(
        PropertyApiClient propertyApiClient)
    {
        _propertyApiClient =
            propertyApiClient;
    }
    [BindProperty]
    public AddPropertyBasicsViewModel Input { get; set; } = new();
    [BindProperty]
    public AddPropertyDetailsViewModel Details { get; set; } = new();
    [BindProperty]
    public List<string> SelectedAmenities { get; set; } = [];
    [BindProperty]
    public IFormFile? CoverPhoto { get; set; }

    [BindProperty]
    public List<IFormFile> GalleryImages { get; set; } = [];

    [BindProperty]
    public List<IFormFile> Videos { get; set; } = [];

    [BindProperty]
    public List<IFormFile> FloorPlans { get; set; } = [];

    [BindProperty]
    public List<IFormFile> Documents { get; set; } = [];
    [BindProperty]
    public int PropertyID { get; set; }

    public int CurrentStep { get; private set; } = 1;

    public PropertyReviewApiResponse? Review { get; private set; }
    [BindProperty]
    public string FormAction { get; set; } = string.Empty;
    public List<AmenityOption> Amenities { get; } =
[

    // SECURITY

    new()
    {
        Name = "24/7 Security",
        Category = "Security",
        IconPath = "~/images/icons/amenities/security.png"
    },

    new()
    {
        Name = "Gated Community",
        Category = "Security",
        IconPath = "~/images/icons/amenities/gated.png"
    },

    new()
    {
        Name = "CCTV Surveillance",
        Category = "Security",
        IconPath = "~/images/icons/amenities/cctv.png"
    },

    new()
    {
        Name = "Security Cabin",
        Category = "Security",
        IconPath = "~/images/icons/amenities/security.png"
    },

    new()
    {
        Name = "Intercom",
        Category = "Security",
        IconPath = "~/images/icons/amenities/gated.png"
    },

    new()
    {
        Name = "Fire Alarm System",
        Category = "Security",
        IconPath = "~/images/icons/amenities/alarm.png"
    },

    new()
    {
        Name = "Fire Sprinklers",
        Category = "Security",
        IconPath = "~/images/icons/amenities/gated.png"
    },

    new()
    {
        Name = "Video Door Phone",
        Category = "Security",
        IconPath = "~/images/icons/amenities/door.png"
    },

    new()
    {
        Name = "Access Control",
        Category = "Security",
        IconPath = "~/images/icons/amenities/control-panel.png"
    },


    // RECREATION


    new()
    {
        Name = "Swimming Pool",
        Category = "Recreation",
        IconPath = "~/images/icons/amenities/swimming-pool.png"
    },

    new()
    {
        Name = "Gym / Fitness Center",
        Category = "Recreation",
        IconPath = "~/images/icons/amenities/gym.png"
    },

    new()
    {
        Name = "Children's Play Area",
        Category = "Recreation",
        IconPath = "~/images/icons/amenities/play-area.png"
    },

    new()
    {
        Name = "Jogging Track",
        Category = "Recreation",
        IconPath = "~/images/icons/amenities/jogging-track.png"
    },

    new()
    {
        Name = "Badminton",
        Category = "Recreation",
        IconPath = "~/images/icons/amenities/badminton.png"
    },


    // UTILITIES


    new()
    {
        Name = "Power Backup",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/power.png"
    },

    new()
    {
        Name = "24-Hour Water Supply",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/water.png"
    },

    new()
    {
        Name = "Elevator / Lift",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/elevator.png"
    },

    new()
    {
        Name = "Rainwater Harvesting",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/heavy-rain.png"
    },

    new()
    {
        Name = "Sewage Treatment Plant",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/gated.png"
    },

    new()
    {
        Name = "Waste Management",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/waste.png"
    },

    new()
    {
        Name = "EV Charging Station",
        Category = "Utilities",
        IconPath = "~/images/icons/amenities/charging-station.png"
    },

    // NEARBY FACILITIES


    new()
    {
        Name = "School",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/school.png"
    },

    new()
    {
        Name = "College",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/college.png"
    },

    new()
    {
        Name = "Hospital",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/hospital.png"
    },

    new()
    {
        Name = "Pharmacy",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/pharmacy.png"
    },

    new()
    {
        Name = "Supermarket",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/market.png"
    },

    new()
    {
        Name = "Shopping Mall",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/shopping-mall.png"
    },

    new()
    {
        Name = "Restaurant",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/restaurant.png"
    },

    new()
    {
        Name = "Bus Stop",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/bus-stop.png"
    },

    new()
    {
        Name = "Metro Station",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/metro.png"
    },

    new()
    {
        Name = "Railway Station",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/railway.png"
    },

    new()
    {
        Name = "Airport",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/airport.png"
    },

    new()
    {
        Name = "Park",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/play-area.png"
    },

    new()
    {
        Name = "Bank / ATM",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/bank.png"
    },

    new()
    {
        Name = "Place of Worship",
        Category = "Nearby facilities",
        IconPath = "~/images/icons/amenities/worship.png"
    }
];
    // save amenities
    private async Task<bool> SaveAmenitiesAsync(
    int propertyId,
    CancellationToken cancellationToken)
    {
        if (SelectedAmenities.Count == 0)
        {
            return false;
        }

        List<AmenityApiModel> allAmenities =
            await _propertyApiClient
                .GetAllAmenitiesAsync(
                    cancellationToken);

        List<int> amenityIds = [];

        foreach (string selectedName in SelectedAmenities)
        {
            AmenityApiModel? amenity =
                allAmenities.FirstOrDefault(
                    x =>
                        string.Equals(
                            x.AmenityName,
                            selectedName,
                            StringComparison.OrdinalIgnoreCase));

            if (amenity == null)
            {
                throw new InvalidOperationException(
                    $"Amenity '{selectedName}' was not found.");
            }

            amenityIds.Add(
                amenity.AmenityID);
        }

        SavePropertyAmenitiesApiRequest request =
            new SavePropertyAmenitiesApiRequest
            {
                AmenityIDs = amenityIds
            };

        ApiMessageResponse response =
            await _propertyApiClient
                .SavePropertyAmenitiesAsync(
                    propertyId,
                    request,
                    cancellationToken);

        return response.Success;
    }
    public async Task<IActionResult> OnGetAsync(
    int? propertyId,
    int? step,
    CancellationToken cancellationToken)
    {
        CurrentStep =
            step ?? 1;

        if (!propertyId.HasValue)
        {
            return Page();
        }

        PropertyID =
            propertyId.Value;


        Review =
            await _propertyApiClient
                .GetPropertyReviewAsync(
                    PropertyID,
                    cancellationToken);


        if (Review == null)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Unable to load property {PropertyID}."
            );

            CurrentStep = 1;

            return Page();
        }

        LoadReviewIntoForm();


        return Page();
    }
    public async Task<IActionResult> OnPostAsync(
    CancellationToken cancellationToken)
    {
        try
        {
            // Step 1 - Basics

            if (FormAction == "ValidateStep1")
            {
                // Clear validation results from other wizard steps

                ModelState.Clear();

                // Validate Step 1 model only

                TryValidateModel(
                    Input,
                    nameof(Input)
                );

                if (!ModelState.IsValid)
                {
                    CurrentStep = 1;

                    return Page();
                }

                // Step 1 valid; move to Step 2

                CurrentStep = 2;

                return Page();
            }


            // Step 2 - Details

            if (FormAction == "ValidateStep2")
            {
                ModelState.Clear();
                ValidateDetailsModel();

                if (!ModelState.IsValid)
                {
                    CurrentStep = 2;
                    return Page();
                }

                CurrentStep = 3;
                return Page();
            }


            //STEP 3 - AMENITIES


            if (FormAction == "ValidateStep3")
            {
                ModelState.Clear();

                /*
                   At least one amenity is required.
                */

                if (SelectedAmenities == null ||
                    SelectedAmenities.Count == 0)
                {
                    ModelState.AddModelError(
                        nameof(SelectedAmenities),
                        "Please select at least one amenity."
                    );
                }

                if (!ModelState.IsValid)
                {
                    CurrentStep = 3;

                    return Page();
                }


                // Step 3 is valid.
                //Move to Step 4.


                CurrentStep = 4;

                return Page();
            }



            //STEP 4 - SAVE PROPERTY


            if (FormAction == "SaveForReview")
            {
                ModelState.Clear();

                TryValidateModel(Input, nameof(Input));
                ValidateDetailsModel(); // Ensures Step 2 rules apply during Step 4 save

                if (SelectedAmenities == null || SelectedAmenities.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Please select at least one amenity.");
                }


                bool hasExistingCover = false;

                bool hasExistingGallery = false;


                if (PropertyID > 0)
                {
                    Review =
                        await _propertyApiClient
                            .GetPropertyReviewAsync(
                                PropertyID,
                                cancellationToken
                            );


                    if (Review != null)
                    {
                        hasExistingCover =
                            Review.Media.Any(
                                x =>
                                    x.MediaType ==
                                    "CoverPhoto"
                            );


                        hasExistingGallery =
                            Review.Media.Any(
                                x =>
                                    x.MediaType ==
                                    "GalleryImage"
                            );
                    }
                }


                if (
                    CoverPhoto == null &&
                    !hasExistingCover
                )
                {
                    ModelState.AddModelError(
                        nameof(CoverPhoto),
                        "Cover photo is required."
                    );
                }


                if (
                    (GalleryImages == null ||
                     GalleryImages.Count == 0) &&
                    !hasExistingGallery
                )
                {
                    ModelState.AddModelError(
                        nameof(GalleryImages),
                        "At least one gallery image is required."
                    );
                }

                if (!ModelState.IsValid)
                {
                    CurrentStep = 4;

                    return Page();
                }

                return await SaveForReviewAsync(
                    cancellationToken
                );
            }


            /* STEP 5 - SUBMIT PROPERTY */

            if (FormAction == "SubmitProperty")
            {


                ModelState.Clear();

                CurrentStep = 5;

                return await SubmitPropertyAsync(
                    cancellationToken
                );
            }


            ModelState.AddModelError(
                string.Empty,
                "Invalid form action."
            );

            CurrentStep = 1;

            return Page();
        }
        catch (HttpRequestException ex)
            when (
                ex.StatusCode ==
                System.Net.HttpStatusCode.Unauthorized)
        {
            return RedirectToPage(
                "/Account/Login"
            );
        }
        catch (HttpRequestException ex)
            when (
                ex.StatusCode ==
                System.Net.HttpStatusCode.Forbidden)
        {
            return RedirectToPage(
                "/Account/AccessDenied"
            );
        }
        catch (TaskCanceledException ex)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Property submission timed out or was cancelled: {ex.Message}"
            );

            CurrentStep = 5;

            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                ex.Message
            );
            CurrentStep = 4;

            return Page();
        }
    }
    private async Task<IActionResult> SaveForReviewAsync(
    CancellationToken cancellationToken)
    {
        try
        {

            CreatePropertyApiRequest request =
    new CreatePropertyApiRequest
    {
        PropertyTitle = Input.PropertyTitle,
        PropertyType = Input.PropertyType,
        ListingType = Input.ListingType,
        PropertyStatus = Input.PropertyStatus,

        LocationAddress = Input.LocationAddress,
        Country = Input.Country,
        State = Input.State,
        City = Input.City,
        ZipCode = Input.ZipCode,

        Price = Details.Price,
        SecurityDeposit = Details.SecurityDeposit,
        Area = Details.Area,
        AreaUnit = Details.AreaUnit,
        Bedrooms = Details.Bedrooms,
        Bathrooms = Details.Bathrooms,

        Balconies = Details.Balconies,
        Floor = Details.Floor,
        ParkingSpaces = Details.ParkingSpaces,
        YearBuilt = Details.YearBuilt,
        PropertyAgeYears = Details.PropertyAgeYears,
        PossessionDate = Details.PossessionDate,

        FurnishingType = Details.FurnishingType,
        FacingDirection = Details.FacingDirection,
        PreferredTenants = Details.PreferredTenants,
        TenantFoodPreference = Details.TenantFoodPreference,
        Description = Details.Description
    };


            int propertyId;


            if (PropertyID == 0)
            {


                CreatePropertyApiResponse response =
               await _propertyApiClient
                .CreatePropertyAsync(
                     request,
                    cancellationToken);


                if (!response.Success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        response.Message
                    );

                    CurrentStep = 4;

                    return Page();
                }


                propertyId =
                    response.PropertyID;
            }
            else
            {


                UpdatePropertyApiRequest updateRequest =
                    new UpdatePropertyApiRequest
                    {
                        PropertyTitle =
                            Input.PropertyTitle,

                        PropertyType =
                            Input.PropertyType,

                        ListingType =
                            Input.ListingType,

                        PropertyStatus =
                            Input.PropertyStatus,

                        LocationAddress =
                            Input.LocationAddress,

                        Country =
                            Input.Country,

                        State =
                            Input.State,

                        City =
                            Input.City,

                        ZipCode =
                            Input.ZipCode,

                        Price =
                            Details.Price,

                        SecurityDeposit =
                            Details.SecurityDeposit,

                        Area =
                            Details.Area,

                        AreaUnit =
                            Details.AreaUnit,

                        Bedrooms =
                            Details.Bedrooms,

                        Bathrooms =
                            Details.Bathrooms,

                        Balconies =
                            Details.Balconies,

                        Floor =
                            Details.Floor,

                        ParkingSpaces =
                            Details.ParkingSpaces,

                        YearBuilt =
                            Details.YearBuilt,

                        PropertyAgeYears =
                            Details.PropertyAgeYears,

                        PossessionDate =
                            Details.PossessionDate,

                        FurnishingType =
                            Details.FurnishingType,

                        FacingDirection =
                            Details.FacingDirection,

                        PreferredTenants =
                            Details.PreferredTenants,

                        TenantFoodPreference =
                            Details.TenantFoodPreference,

                        Description =
                            Details.Description
                    };


                ApiMessageResponse updateResponse =
                    await _propertyApiClient
                        .UpdatePropertyAsync(
                            PropertyID,
                            updateRequest,
                            cancellationToken);


                if (!updateResponse.Success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to update the property."
                    );

                    CurrentStep = 4;

                    return Page();
                }


                propertyId =
                    PropertyID;
            }




            bool amenitiesSaved =
                await SaveAmenitiesAsync(
                    propertyId,
                    cancellationToken);


            if (!amenitiesSaved)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Property was created, but saving amenities failed."
                );

                CurrentStep = 4;

                return Page();
            }


            bool hasNewMedia =
     CoverPhoto != null ||
     GalleryImages.Count > 0 ||
     Videos.Count > 0 ||
     FloorPlans.Count > 0 ||
     Documents.Count > 0;


            if (PropertyID == 0 || hasNewMedia)
            {
                /*
                   New property:
                   media is mandatory.

                   Existing property:
                   upload only when seller selected
                   new media.
                */

                ApiMessageResponse mediaResponse =
                    await _propertyApiClient
                        .UploadPropertyMediaAsync(
                            propertyId,
                            CoverPhoto,
                            GalleryImages,
                            Videos,
                            FloorPlans,
                            Documents,
                            cancellationToken);


                if (!mediaResponse.Success)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        mediaResponse.Message
                    );

                    CurrentStep = 4;

                    return Page();
                }
            }


            return RedirectToPage(
                "./Index",
                new
                {
                    propertyId = propertyId,
                    step = 5
                }
            );
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Step 4 failed: {ex.Message}"
            );

            CurrentStep = 4;

            return Page();
        }
    }
    private async Task<IActionResult> SubmitPropertyAsync(
      CancellationToken cancellationToken)
    {


        using CancellationTokenSource submitCts =
            new CancellationTokenSource(
                TimeSpan.FromSeconds(30)
            );

        try
        {
            SubmitPropertyApiResponse response =
                await _propertyApiClient.SubmitPropertyAsync(
                    PropertyID,
                    submitCts.Token
                );


            if (!response.Success)
            {
                foreach (string error in response.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error
                    );
                }


                CurrentStep = 5;


                Review =
                    await _propertyApiClient.GetPropertyReviewAsync(
                        PropertyID,
                        cancellationToken
                    );


                return Page();
            }


            return RedirectToPage(
                "./Index",
                new
                {
                    propertyId = PropertyID,
                    step = 5,
                    success = "true"
                }
            );
        }
        catch (OperationCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Property submission took too long. Please try again."
            );

            CurrentStep = 5;

            return Page();
        }
    }
    private void LoadReviewIntoForm()
    {
        if (Review?.Property == null)
        {
            return;
        }


        PropertyApiModel property =
            Review.Property;


        Input.PropertyTitle =
            property.PropertyTitle;

        Input.PropertyType =
            property.PropertyType;

        Input.ListingType =
            property.ListingType;

        Input.PropertyStatus =
            property.PropertyStatus;

        Input.LocationAddress =
            property.LocationAddress;

        Input.Country =
            property.Country;

        Input.State =
            property.State;

        Input.City =
            property.City;

        Input.ZipCode =
            property.ZipCode;


        Details.Price =
            property.Price;

        Details.SecurityDeposit =
            property.SecurityDeposit;

        Details.Area =
            property.Area;

        Details.AreaUnit =
            property.AreaUnit;

        Details.Bedrooms =
            property.Bedrooms;

        Details.Bathrooms =
            property.Bathrooms;

        Details.Balconies =
            property.Balconies;

        Details.Floor =
            property.Floor;

        Details.ParkingSpaces =
            property.ParkingSpaces;

        Details.YearBuilt =
            property.YearBuilt;

        Details.PropertyAgeYears =
            property.PropertyAgeYears;

        Details.PossessionDate =
            property.PossessionDate;

        Details.FurnishingType =
            property.FurnishingType;

        Details.FacingDirection =
            property.FacingDirection;

        Details.PreferredTenants =
            property.PreferredTenants;

        Details.TenantFoodPreference =
            property.TenantFoodPreference;

        Details.Description =
            property.Description;


        /*
           STEP 3 - AMENITIES */

        SelectedAmenities =
            Review.Amenities
                .Select(
                    x => x.AmenityName
                )
                .ToList();
    }
    private void ValidateDetailsModel()
    {
        TryValidateModel(Details, nameof(Details));

        // Validate Year Built directly without string re-parsing
        if (Details.YearBuilt < 1950 || Details.YearBuilt > DateTime.Today.Year)
        {
            ModelState.AddModelError(
                "Details.YearBuilt",
                "Please select a valid year built between 1950 and today."
            );
        }

        // Validate Possession Date directly without culture-dependent string parsing
        if (!Details.PossessionDate.HasValue)
        {
            ModelState.AddModelError(
                "Details.PossessionDate",
                "Possession date is a required field."
            );
        }
        else if (Details.PossessionDate.Value.Date <= DateTime.Today)
        {
            ModelState.AddModelError(
                "Details.PossessionDate",
                "Possession date must be after today."
            );
        }
    }
}
