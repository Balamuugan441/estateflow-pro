using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.DTOs.Home;
using Brokerage.Models.DTOs.Properties;
using Brokerage.Models.Entities;
using Dapper;
using System.Data;

namespace Brokerage.Data.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public PropertyRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Creates a new property using the property details collected from the seller.
    public async Task<int> CreatePropertyAsync(Property property)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
    "dbo.usp_Property_Create",
    new
    {
        property.SellerID,
        property.PropertyTitle,
        property.PropertyType,
        property.ListingType,
        property.PropertyStatus,
        property.ListingStatus,
        property.LocationAddress,
        property.Country,
        property.State,
        property.City,
        property.ZipCode,
        property.Price,
        property.SecurityDeposit,
        property.Area,
        property.AreaUnit,
        property.Bedrooms,
        property.Bathrooms,
        property.Balconies,
        property.Floor,
        property.ParkingSpaces,
        property.YearBuilt,
        property.PropertyAgeYears,
        property.PossessionDate,
        property.FurnishingType,
        property.FacingDirection,
        property.PreferredTenants,
        property.TenantFoodPreference,
        property.Description
    },
    commandType: CommandType.StoredProcedure);
    }

    // Retrieves a property by its database PropertyID.
    public async Task<Property?> GetPropertyByIdAsync(int propertyId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Property>(
            "dbo.usp_Property_Get",
            new
            {
                PropertyID = propertyId
            },
            commandType: CommandType.StoredProcedure);
    }

    // Retrieves a property by its public PropertyGUID.
    public async Task<Property?> GetPropertyByGuidAsync(Guid propertyGuid)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Property>(
            "dbo.usp_Property_Get",
            new
            {
                PropertyGUID = propertyGuid
            },
            commandType: CommandType.StoredProcedure);
    }

    // Retrieves all properties created by a particular seller.
    public async Task<IEnumerable<Property>> GetPropertiesBySellerAsync(int sellerId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Property>(
            "dbo.usp_Property_GetBySeller",
            new
            {
                SellerID = sellerId
            },
            commandType: CommandType.StoredProcedure);
    }

    // Updates the editable property details and returns whether the database update succeeded.
    public async Task<bool> UpdatePropertyAsync(Property property)
    {
        using var connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Property_Update",
            new
            {
                property.PropertyID,
                property.PropertyTitle,
                property.PropertyType,
                property.ListingType,
                property.PropertyStatus,
                property.LocationAddress,
                property.Country,
                property.State,
                property.City,
                property.ZipCode,
                property.Price,
                property.SecurityDeposit,
                property.Area,
                property.AreaUnit,
                property.Bedrooms,
                property.Bathrooms,
                property.Balconies,
                property.Floor,
                property.ParkingSpaces,
                property.YearBuilt,
                property.PropertyAgeYears,
                property.PossessionDate,
                property.FurnishingType,
                property.FacingDirection,
                property.PreferredTenants,
                property.TenantFoodPreference,
                property.Description
            },
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    // Updates a property's listing status and returns whether the database changed the property.
    public async Task<bool> UpdateListingStatusAsync(
        int propertyId,
        string listingStatus)
    {
        using var connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Property_UpdateListingStatus",
            new
            {
                PropertyID = propertyId,
                ListingStatus = listingStatus,
                ExpectedCurrentStatus = (string?)null
            },
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    // Retrieves all available amenities or the amenities assigned to a specific property.
    public async Task<IEnumerable<Amenity>> GetAllAmenitiesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Amenity>(
            "dbo.usp_Amenity_Get",
            new
            {
                PropertyID = (int?)null
            },
            commandType: CommandType.StoredProcedure);
    }

    // Replaces the complete amenity selection for a property inside one database transaction.
    public async Task<bool> SavePropertyAmenitiesAsync(
        int propertyId,
        IEnumerable<int> amenityIds)
    {
        DataTable amenityTable = CreateIntListTable(amenityIds);

        using var connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            "dbo.usp_PropertyAmenities_Replace",
            new
            {
                PropertyID = propertyId,
                AmenityIDs = amenityTable.AsTableValuedParameter(
                    "dbo.IntListTableType")
            },
            commandType: CommandType.StoredProcedure);

        return true;
    }

    // Inserts each uploaded media record using the common property media insert procedure.
    public async Task<bool> SavePropertyMediaAsync(
        IEnumerable<PropertyMedia> media)
    {
        using var connection = _connectionFactory.CreateConnection();

        foreach (PropertyMedia item in media)
        {
            await connection.ExecuteAsync(
                "dbo.usp_PropertyMedia_Insert",
                new
                {
                    item.PropertyID,
                    item.MediaType,
                    item.FileName,
                    item.FilePath,
                    item.ContentType,
                    item.FileSizeBytes,
                    item.DisplayOrder
                },
                commandType: CommandType.StoredProcedure);
        }

        return true;
    }

    // Updates an existing cover photo while forcing its display order to one.
    public async Task<bool> UpdatePropertyCoverMediaAsync(
        PropertyMedia media)
    {
        using var connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteAsync(
            "dbo.usp_PropertyMedia_Update",
            new
            {
                media.MediaID,
                media.PropertyID,
                MediaType = "CoverPhoto",
                media.FileName,
                media.FilePath,
                media.ContentType,
                media.FileSizeBytes
            },
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    // Updates an existing gallery, video, floor plan, or document without changing its display order.

    public async Task<bool> UpdatePropertyMediaAsync(
        PropertyMedia media)
    {
        using var connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteAsync(
            "dbo.usp_PropertyMedia_Update",
            new
            {
                media.MediaID,
                media.PropertyID,
                media.MediaType,
                media.FileName,
                media.FilePath,
                media.ContentType,
                media.FileSizeBytes
            },
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    // Deletes an existing non-cover media record and returns whether SQL Server removed the row.
    public async Task<bool> DeletePropertyMediaAsync(
        int propertyId,
        int mediaId)
    {
        using var connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteScalarAsync<int>(
            "dbo.usp_PropertyMedia_Delete",
            new
            {
                PropertyID = propertyId,
                MediaID = mediaId
            },
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    // Retrieves all media associated with a property using the existing media display ordering.
    public async Task<IEnumerable<PropertyMedia>> GetPropertyMediaAsync(
        int propertyId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PropertyMedia>(
            "dbo.usp_PropertyMedia_Get",
            new
            {
                PropertyID = propertyId
            },
            commandType: CommandType.StoredProcedure);
    }

    // Retrieves amenities assigned to a particular property.
    public async Task<IEnumerable<Amenity>> GetPropertyAmenitiesAsync(int propertyId)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Amenity>(
            "dbo.usp_Amenity_Get",
            new
            {
                PropertyID = propertyId
            },
            commandType: CommandType.StoredProcedure);
    }

    // Retrieves filtered and paginated property records together with admin property statistics.
    public async Task<AdminPropertyListResponse> GetPropertiesForAdminAsync(
        AdminPropertyQueryRequest request)
    {
        const int pageSize = 5;

        int pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        string? search = request.Search?.Trim();

        using var connection = _connectionFactory.CreateConnection();

        var parameters = new
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = string.IsNullOrWhiteSpace(search)
                ? null
                : $"%{search}%",
            ListingStatus = string.IsNullOrWhiteSpace(request.ListingStatus)
                ? null
                : request.ListingStatus,
            PropertyType = string.IsNullOrWhiteSpace(request.PropertyType)
                ? null
                : request.PropertyType,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice
        };

        using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            "dbo.usp_Admin_Property_GetPaged",
            parameters,
            commandType: CommandType.StoredProcedure);

        int totalRecords = await grid.ReadSingleAsync<int>();

        AdminPropertyListResponse stats =
            await grid.ReadSingleAsync<AdminPropertyListResponse>();

        IEnumerable<AdminPropertyResponse> properties =
            await grid.ReadAsync<AdminPropertyResponse>();

        List<AdminPropertyResponse> propertyList = properties.ToList();

        int totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);

        return new AdminPropertyListResponse
        {
            Properties = propertyList,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            TotalLive = stats.TotalLive,
            TotalPending = stats.TotalPending,
            TotalRejected = stats.TotalRejected
        };
    }

    // Retrieves pending properties for admin approval using the existing three-card page size.
    public async Task<AdminPendingApprovalListResponse> GetPendingApprovalsAsync(int pageNumber)

    {
        const int pageSize = 3;

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        using var connection = _connectionFactory.CreateConnection();

        using SqlMapper.GridReader grid = await connection.QueryMultipleAsync(
            "dbo.usp_Admin_Property_GetPending",
            new
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            commandType: CommandType.StoredProcedure);

        int totalRecords = await grid.ReadSingleAsync<int>();

        IEnumerable<AdminPendingApprovalResponse> properties =
            await grid.ReadAsync<AdminPendingApprovalResponse>();

        int totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);

        return new AdminPendingApprovalListResponse
        {
            Properties = properties.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    // Updates a pending property's status only when its current status is still Pending.
    public async Task<bool> UpdatePendingPropertyStatusAsync(
        int propertyId,
        string listingStatus)
    {
        using var connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Property_UpdateListingStatus",
            new
            {
                PropertyID = propertyId,
                ListingStatus = listingStatus,
                ExpectedCurrentStatus = "Pending"
            },
            commandType: CommandType.StoredProcedure);

        return rowsAffected > 0;
    }

    // Retrieves the total number of properties currently approved for public listing.
    public async Task<int> GetApprovedPropertyCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            "dbo.usp_Property_GetApprovedCount",
            commandType: CommandType.StoredProcedure);
    }

    // Retrieves the latest approved properties optionally filtered by city.
    public async Task<IEnumerable<PublicPropertyResponse>> GetLatestApprovedPropertiesAsync(
        string? city,
        int count)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PublicPropertyResponse>(
            "dbo.usp_Property_GetLatestApproved",
            new
            {
                City = string.IsNullOrWhiteSpace(city)
                    ? null
                    : city.Trim(),
                Count = count
            },
            commandType: CommandType.StoredProcedure);
    }

    // Retrieves approved buyer properties using search, location, price, type, bedroom, listing type, sorting, and pagination filters.
    public async Task<BuyerPropertyListResponse> SearchApprovedPropertiesAsync(
        BuyerPropertySearchRequest request,
        int buyerId,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 6;

        int pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        int offset = (pageNumber - 1) * pageSize;

        string? search = string.IsNullOrWhiteSpace(request.Search)
            ? null
            : request.Search.Trim();

        string? locationType = string.IsNullOrWhiteSpace(request.LocationType)
            ? null
            : request.LocationType.Trim();

        string? locationValue = string.IsNullOrWhiteSpace(request.LocationValue)
            ? null
            : request.LocationValue.Trim();

        string sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "latest"
            : request.SortBy.Trim().ToLowerInvariant();

        List<string> listingTypes = request.ListingTypes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (sortBy != "latest" &&
            sortBy != "price-low" &&
            sortBy != "price-high")
        {
            sortBy = "latest";
        }

        DataTable listingTypeTable = CreateStringListTable(listingTypes);

        DynamicParameters parameters = new DynamicParameters();

        parameters.Add(
            "UserID",
            buyerId);

        parameters.Add(
            "SearchPattern",
            search == null
                ? null
                : $"{search}%");

        parameters.Add(
            "LocationType",
            locationType);

        parameters.Add(
            "LocationValue",
            locationValue);

        parameters.Add(
            "MinPrice",
            request.MinPrice);

        parameters.Add(
            "MaxPrice",
            request.MaxPrice);

        parameters.Add(
            "PropertyType",
            string.IsNullOrWhiteSpace(request.PropertyType)
                ? null
                : request.PropertyType.Trim());

        parameters.Add(
            "MinBedrooms",
            request.MinBedrooms);

        parameters.Add(
            "HasListingTypeFilter",
            listingTypes.Count > 0);

        parameters.Add(
            "ListingTypes",
            listingTypeTable.AsTableValuedParameter(
                "dbo.StringListTableType"));

        parameters.Add(
            "SortBy",
            sortBy);

        parameters.Add(
            "Offset",
            offset);

        parameters.Add(
            "PageSize",
            pageSize);

        using var connection = _connectionFactory.CreateConnection();

        CommandDefinition command = new(
            "dbo.usp_Buyer_Property_SearchApproved",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using SqlMapper.GridReader grid =
            await connection.QueryMultipleAsync(command);

        int totalRecords = await grid.ReadSingleAsync<int>();

        List<BuyerPropertyCardRow> propertyRows =
            (
                await grid.ReadAsync<BuyerPropertyCardRow>()
            ).ToList();

        if (propertyRows.Count == 0)
        {
            return new BuyerPropertyListResponse
            {
                Properties = [],
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = CalculateTotalPages(
                    totalRecords,
                    pageSize)
            };
        }

        List<int> propertyIds = propertyRows
            .Select(x => x.PropertyID)
            .ToList();

        DataTable propertyIdTable = CreateIntListTable(propertyIds);

        DynamicParameters detailParameters = new DynamicParameters();

        detailParameters.Add(
            "PropertyIDs",
            propertyIdTable.AsTableValuedParameter(
                "dbo.IntListTableType"));

        CommandDefinition detailCommand = new(
            "dbo.usp_Buyer_Property_GetDetails",
            detailParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using SqlMapper.GridReader detailGrid =
            await connection.QueryMultipleAsync(detailCommand);

        List<BuyerPropertyAmenityRow> amenityRows =
            (
                await detailGrid.ReadAsync<BuyerPropertyAmenityRow>()
            ).ToList();

        List<BuyerPropertyMediaRow> mediaRows =
            (
                await detailGrid.ReadAsync<BuyerPropertyMediaRow>()
            ).ToList();

        Dictionary<int, List<BuyerPropertyAmenityDto>> amenitiesByProperty =
            amenityRows
                .GroupBy(x => x.PropertyID)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(x => new BuyerPropertyAmenityDto
                        {
                            AmenityID = x.AmenityID,
                            AmenityName = x.AmenityName,
                            Category = x.Category
                        })
                        .ToList());

        Dictionary<int, List<BuyerPropertyMediaDto>> mediaByProperty =
            mediaRows
                .GroupBy(x => x.PropertyID)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(x => new BuyerPropertyMediaDto
                        {
                            MediaID = x.MediaID,
                            MediaType = x.MediaType,
                            FileName = x.FileName,
                            FilePath = x.FilePath,
                            DisplayOrder = x.DisplayOrder
                        })
                        .ToList());

        List<BuyerPropertyCardDto> properties =
            propertyRows
                .Select(row => new BuyerPropertyCardDto
                {
                    PropertyID = row.PropertyID,
                    PropertyGUID = row.PropertyGUID,
                    PropertyTitle = row.PropertyTitle,
                    PropertyType = row.PropertyType,
                    ListingType = row.ListingType,
                    LocationAddress = row.LocationAddress,
                    Country = row.Country,
                    State = row.State,
                    City = row.City,
                    ZipCode = row.ZipCode,
                    Price = row.Price,
                    Area = row.Area,
                    AreaUnit = row.AreaUnit,
                    Bedrooms = row.Bedrooms,
                    Bathrooms = row.Bathrooms,
                    CoverImagePath = row.CoverImagePath,
                    IsFavorite = row.IsFavorite,
                    Amenities = amenitiesByProperty.TryGetValue(
                        row.PropertyID,
                        out List<BuyerPropertyAmenityDto>? amenities)
                        ? amenities
                        : [],
                    Media = mediaByProperty.TryGetValue(
                        row.PropertyID,
                        out List<BuyerPropertyMediaDto>? media)
                        ? media
                        : []
                })
                .ToList();

        return new BuyerPropertyListResponse
        {
            Properties = properties,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = CalculateTotalPages(
                totalRecords,
                pageSize)
        };
    }

    // Converts an integer collection into the table-valued parameter format expected by SQL Server.
    private static DataTable CreateIntListTable(IEnumerable<int> values)
    {
        DataTable table = new DataTable();

        table.Columns.Add(
            "ID",
            typeof(int));

        foreach (int value in values)
        {
            DataRow row = table.NewRow();

            row["ID"] = value;

            table.Rows.Add(row);
        }

        return table;
    }

    // Converts a string collection into the table-valued parameter format expected by SQL Server.
    private static DataTable CreateStringListTable(IEnumerable<string> values)
    {
        DataTable table = new DataTable();

        table.Columns.Add(
            "ItemValue",
            typeof(string));

        foreach (string value in values)
        {
            DataRow row = table.NewRow();

            row["ItemValue"] = value;

            table.Rows.Add(row);
        }

        return table;
    }

    // Calculates the number of pages from the total matching record count.
    private static int CalculateTotalPages(
        int totalRecords,
        int pageSize)
    {
        return totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);
    }

    // Holds the property columns returned by the buyer property search procedure.
    private sealed class BuyerPropertyCardRow
    {
        public int PropertyID { get; set; }

        public Guid PropertyGUID { get; set; }

        public string PropertyTitle { get; set; } = string.Empty;

        public string PropertyType { get; set; } = string.Empty;

        public string ListingType { get; set; } = string.Empty;

        public string? LocationAddress { get; set; }

        public string? Country { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? ZipCode { get; set; }

        public decimal Price { get; set; }

        public decimal Area { get; set; }

        public string? AreaUnit { get; set; }

        public string Bedrooms { get; set; } = string.Empty;

        public decimal Bathrooms { get; set; }

        public string? CoverImagePath { get; set; }

        public bool IsFavorite { get; set; }
    }

    // Holds the amenity columns returned for the properties displayed on the buyer page.
    private sealed class BuyerPropertyAmenityRow
    {
        public int PropertyID { get; set; }

        public int AmenityID { get; set; }

        public string AmenityName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }

    // Holds the media columns returned for the properties displayed on the buyer page.
    private sealed class BuyerPropertyMediaRow
    {
        public int MediaID { get; set; }

        public int PropertyID { get; set; }

        public string MediaType { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public int? DisplayOrder { get; set; }
    }
}