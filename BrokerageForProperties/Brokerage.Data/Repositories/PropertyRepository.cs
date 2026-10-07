using Brokerage.Data.Database;
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Admin;
using Brokerage.Models.DTOs.Home;
using Brokerage.Models.DTOs.Properties;
using Brokerage.Models.Entities;
using Dapper;

namespace Brokerage.Data.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public PropertyRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    //Post Property in Database
    // Gets Property Details of Step 1 and Step 2 from the database Creates a Property as a draft stores it in the database and retiurns the propertyID
    public async Task<int> CreatePropertyAsync(Property property)
    {
        const string query = """
        INSERT INTO Properties
        (
            SellerID,
            PropertyTitle,
            PropertyType,
            ListingType,
            PropertyStatus,
            ListingStatus,
            LocationAddress,
            Country,
            State,
            City,
            ZipCode,
            Price,
            SecurityDeposit,
            Area,
            AreaUnit,
            Bedrooms,
            Bathrooms,
            Balconies,
            Floor,
            ParkingSpaces,
            YearBuilt,
            PropertyAgeYears,
            PossessionDate,
            FurnishingType,
            FacingDirection,
            PreferredTenants,
            TenantFoodPreference,
            Description
        )
        OUTPUT INSERTED.PropertyID
        VALUES
        (
            @SellerID,
            @PropertyTitle,
            @PropertyType,
            @ListingType,
            @PropertyStatus,
            @ListingStatus,
            @LocationAddress,
            @Country,
            @State,
            @City,
            @ZipCode,
            @Price,
            @SecurityDeposit,
            @Area,
            @AreaUnit,
            @Bedrooms,
            @Bathrooms,
            @Balconies,
            @Floor,
            @ParkingSpaces,
            @YearBuilt,
            @PropertyAgeYears,
            @PossessionDate,
            @FurnishingType,
            @FacingDirection,
            @PreferredTenants,
            @TenantFoodPreference,
            @Description
        );
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            query,
            property);
    }
    public async Task<Property?> GetPropertyByIdAsync(
    int propertyId)
    {
        const string query = """
        SELECT
            PropertyID,
            PropertyGUID,
            SellerID,
            PropertyTitle,
            PropertyType,
            ListingType,
            PropertyStatus,
            ListingStatus,
            LocationAddress,
            Country,
            State,
            City,
            ZipCode,
            Price,
            SecurityDeposit,
            Area,
            AreaUnit,
            Bedrooms,
            Bathrooms,
            Balconies,
            Floor,
            ParkingSpaces,
            YearBuilt,
            PropertyAgeYears,
            PossessionDate,
            FurnishingType,
            FacingDirection,
            PreferredTenants,
            TenantFoodPreference,
            Description,
            CreatedAt,
            UpdatedAt
        FROM Properties
        WHERE PropertyID = @PropertyID;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<Property>(
            query,
            new { PropertyID = propertyId });
    }
    //Returns The Property Details Whose ID matches the Supplied ID
    public async Task<Property?> GetPropertyByGuidAsync(
    Guid propertyGuid)
    {
        const string query = """
        SELECT
            PropertyID,
            PropertyGUID,
            SellerID,
            PropertyTitle,
            PropertyType,
            ListingType,
            PropertyStatus,
            ListingStatus,
            LocationAddress,
            Country,
            State,
            City,
            ZipCode,
            Price,
            SecurityDeposit,
            Area,
            AreaUnit,
            Bedrooms,
            Bathrooms,
            Balconies,
            Floor,
            ParkingSpaces,
            YearBuilt,
            PropertyAgeYears,
            PossessionDate,
            FurnishingType,
            FacingDirection,
            PreferredTenants,
            TenantFoodPreference,
            Description,
            CreatedAt,
            UpdatedAt
        FROM Properties
        WHERE PropertyGUID = @PropertyGUID;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection
            .QuerySingleOrDefaultAsync<Property>(
                query,
                new
                {
                    PropertyGUID = propertyGuid
                });
    }
    //Returns all the Property Details of a Particualr seller
    public async Task<IEnumerable<Property>> GetPropertiesBySellerAsync(
    int sellerId)
    {
        const string query = """
        SELECT
            PropertyID,
            PropertyGUID,
            SellerID,
            PropertyTitle,
            PropertyType,
            ListingType,
            PropertyStatus,
            ListingStatus,
            LocationAddress,
            Country,
            State,
            City,
            ZipCode,
            Price,
            SecurityDeposit,
            Area,
            AreaUnit,
            Bedrooms,
            Bathrooms,
            Balconies,
            Floor,
            ParkingSpaces,
            YearBuilt,
            PropertyAgeYears,
            PossessionDate,
            FurnishingType,
            FacingDirection,
            PreferredTenants,
            TenantFoodPreference,
            Description,
            CreatedAt,
            UpdatedAt
        FROM Properties
        WHERE SellerID = @SellerID
        ORDER BY CreatedAt DESC;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Property>(
            query,
            new { SellerID = sellerId });
    }
    //Used To Update the Property Details Which is already Created
    public async Task<bool> UpdatePropertyAsync(Property property)
    {
        const string query = """
        UPDATE Properties
        SET
            PropertyTitle = @PropertyTitle,
            PropertyType = @PropertyType,
            ListingType = @ListingType,
            PropertyStatus = @PropertyStatus,
            LocationAddress = @LocationAddress,
            Country = @Country,
            State = @State,
            City = @City,
            ZipCode = @ZipCode,
            Price = @Price,
            SecurityDeposit = @SecurityDeposit,
            Area = @Area,
            AreaUnit = @AreaUnit,
            Bedrooms = @Bedrooms,
            Bathrooms = @Bathrooms,
            Balconies = @Balconies,
            Floor = @Floor,
            ParkingSpaces = @ParkingSpaces,
            YearBuilt = @YearBuilt,
            PropertyAgeYears = @PropertyAgeYears,
            PossessionDate = @PossessionDate,
            FurnishingType = @FurnishingType,
            FacingDirection = @FacingDirection,
            PreferredTenants = @PreferredTenants,
            TenantFoodPreference = @TenantFoodPreference,
            Description = @Description,
            UpdatedAt = GETUTCDATE()
        WHERE PropertyID = @PropertyID;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                property);

        return rowsAffected > 0;
    }
    // Updates The Property Listing Status From Draft->Pending->Approved/Rejected->Completed/Sold
    public async Task<bool> UpdateListingStatusAsync(
    int propertyId,
    string listingStatus)
    {
        const string query = """
        UPDATE Properties
        SET
            ListingStatus = @ListingStatus,
            UpdatedAt = GETUTCDATE()
        WHERE PropertyID = @PropertyID;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                new
                {
                    PropertyID = propertyId,
                    ListingStatus = listingStatus
                });

        return rowsAffected > 0;
    }
    public async Task<IEnumerable<Amenity>> GetAllAmenitiesAsync()
    {
        const string query = """
        SELECT
            AmenityID,
            Category,
            AmenityName,
            CreatedAt
        FROM Amenities
        ORDER BY Category, AmenityName;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Amenity>(query);
    }
    //Used to save the Amenities details to database
    //Inserts In the Amenities Table The seller ID and the Amenity Id he selected
    public async Task<bool> SavePropertyAmenitiesAsync(
    int propertyId,
    IEnumerable<int> amenityIds)
    {
        using var connection =
            _connectionFactory.CreateConnection();
        connection.Open();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string deleteQuery = """
            DELETE FROM PropertyAmenities
            WHERE PropertyID = @PropertyID;
            """;

            await connection.ExecuteAsync(
                deleteQuery,
                new { PropertyID = propertyId },
                transaction);


            const string insertQuery = """
            INSERT INTO PropertyAmenities
            (
                PropertyID,
                AmenityID
            )
            VALUES
            (
                @PropertyID,
                @AmenityID
            );
            """;

            foreach (int amenityId in amenityIds)
            {
                await connection.ExecuteAsync(
                    insertQuery,
                    new
                    {
                        PropertyID = propertyId,
                        AmenityID = amenityId
                    },
                    transaction);
            }

            transaction.Commit();

            return true;
        }
        catch
        {
            transaction.Rollback();

            throw;
        }
    }
    //Stores the Property Media Information into the database Such as Its file path ,Id, Type, Name, GUID etc..
    public async Task<bool> SavePropertyMediaAsync(
    IEnumerable<PropertyMedia> media)
    {
        const string query = """
        INSERT INTO PropertyMedia
        (
            PropertyID,
            MediaType,
            FileName,
            FilePath,
            ContentType,
            FileSizeBytes,
            DisplayOrder
        )
        VALUES
        (
            @PropertyID,
            @MediaType,
            @FileName,
            @FilePath,
            @ContentType,
            @FileSizeBytes,
            @DisplayOrder
        );
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        foreach (PropertyMedia item in media)
        {
            await connection.ExecuteAsync(
                query,
                item);
        }

        return true;
    }
    public async Task<bool> UpdatePropertyCoverMediaAsync(
    PropertyMedia media)
    {
        const string query = """
        UPDATE PropertyMedia
        SET
            FileName = @FileName,
            FilePath = @FilePath,
            ContentType = @ContentType,
            FileSizeBytes = @FileSizeBytes,
            DisplayOrder = 1
        WHERE
            MediaID = @MediaID
            AND PropertyID = @PropertyID
            AND MediaType = 'CoverPhoto';
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                media);

        return rowsAffected > 0;
    }
    public async Task<bool> UpdatePropertyMediaAsync(
    PropertyMedia media)
    {
        const string query = """
        UPDATE PropertyMedia
        SET
            FileName = @FileName,
            FilePath = @FilePath,
            ContentType = @ContentType,
            FileSizeBytes = @FileSizeBytes
        WHERE
            MediaID = @MediaID
            AND PropertyID = @PropertyID
            AND MediaType = @MediaType;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                media);

        return rowsAffected > 0;
    }
    public async Task<bool> DeletePropertyMediaAsync(
    int propertyId,
    int mediaId)
    {
        const string query = """
        DELETE FROM PropertyMedia
        WHERE
            PropertyID = @PropertyID
            AND MediaID = @MediaID
            AND MediaType <> 'CoverPhoto';
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                new
                {
                    PropertyID = propertyId,
                    MediaID = mediaId
                });

        return rowsAffected > 0;
    }
    // Fetches the media details of the particular Property
    public async Task<IEnumerable<PropertyMedia>> GetPropertyMediaAsync(
    int propertyId)
    {
        const string query = """
        SELECT
            MediaID,
            PropertyID,
            MediaType,
            FileName,
            FilePath,
            ContentType,
            FileSizeBytes,
            DisplayOrder,
            CreatedAt
        FROM PropertyMedia
        WHERE PropertyID = @PropertyID
        ORDER BY
            CASE
                WHEN MediaType = 'CoverPhoto' THEN 1
                WHEN MediaType = 'GalleryImage' THEN 2
                WHEN MediaType = 'Video' THEN 3
                WHEN MediaType = 'FloorPlan' THEN 4
                WHEN MediaType = 'Document' THEN 5
                ELSE 6
            END,
            DisplayOrder,
            MediaID;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PropertyMedia>(
            query,
            new
            {
                PropertyID = propertyId
            });
    }
    public async Task<IEnumerable<Amenity>> GetPropertyAmenitiesAsync(int propertyId)
    {
        const string query = """
        SELECT
            a.AmenityID,
            a.Category,
            a.AmenityName,
            a.CreatedAt
        FROM PropertyAmenities pa
        INNER JOIN Amenities a
            ON pa.AmenityID = a.AmenityID
        WHERE pa.PropertyID = @PropertyID
        ORDER BY
            a.Category,
            a.AmenityName;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<Amenity>(
            query,
            new
            {
                PropertyID = propertyId
            });

    }
    public async Task<AdminPropertyListResponse>
    GetPropertiesForAdminAsync(
        AdminPropertyQueryRequest request)
    {
        const int pageSize = 5;

        int pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        int offset =
            (pageNumber - 1) * pageSize;

        List<string> filters = [];

        DynamicParameters parameters =
            new DynamicParameters();

        string? search =
            request.Search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            filters.Add(
                """
            (
                p.PropertyTitle LIKE @Search
                OR p.City LIKE @Search
                OR p.State LIKE @Search
                OR p.LocationAddress LIKE @Search
            )
            """);

            parameters.Add(
                "Search",
                $"%{search}%");
        }

        if (!string.IsNullOrWhiteSpace(
            request.ListingStatus))
        {
            filters.Add(
                "p.ListingStatus = @ListingStatus");

            parameters.Add(
                "ListingStatus",
                request.ListingStatus);
        }

        if (!string.IsNullOrWhiteSpace(
            request.PropertyType))
        {
            filters.Add(
                "p.PropertyType = @PropertyType");

            parameters.Add(
                "PropertyType",
                request.PropertyType);
        }

        if (request.MinPrice.HasValue)
        {
            filters.Add(
                "p.Price >= @MinPrice");

            parameters.Add(
                "MinPrice",
                request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            filters.Add(
                "p.Price <= @MaxPrice");

            parameters.Add(
                "MaxPrice",
                request.MaxPrice.Value);
        }

        string whereClause =
            filters.Count > 0
                ? "WHERE " +
                  string.Join(
                      " AND ",
                      filters)
                : string.Empty;

        using var connection =
            _connectionFactory.CreateConnection();

        string countSql = $"""
        SELECT COUNT(1)
        FROM Properties p
        {whereClause};
        """;

        int totalRecords =
            await connection.ExecuteScalarAsync<int>(
                countSql,
                parameters);

        const string statsSql = """
        SELECT
            COUNT(
                CASE
                    WHEN ListingStatus = 'Approved'
                    THEN 1
                END) AS TotalLive,

            COUNT(
                CASE
                    WHEN ListingStatus = 'Pending'
                    THEN 1
                END) AS TotalPending,

            COUNT(
                CASE
                    WHEN ListingStatus = 'Rejected'
                    THEN 1
                END) AS TotalRejected
        FROM Properties;
        """;

        AdminPropertyListResponse stats =
            await connection.QuerySingleAsync<AdminPropertyListResponse>(
                statsSql);

        string dataSql = $"""
    SELECT
        p.PropertyID,
        p.PropertyGUID,
        p.SellerID,
        u.FullName AS SellerName,
        p.PropertyTitle,
        p.PropertyType,
        p.ListingType,
        p.ListingStatus,
        p.LocationAddress,
        p.City,
        p.State,
        p.Price,
        p.CreatedAt,
        cover.FilePath AS CoverImagePath
    FROM Properties p
    INNER JOIN Users u
        ON p.SellerID = u.UserID
    OUTER APPLY
    (
        SELECT TOP 1
            pm.FilePath
        FROM PropertyMedia pm
        WHERE pm.PropertyID = p.PropertyID
          AND pm.MediaType = 'CoverPhoto'
        ORDER BY
            CASE
                WHEN pm.DisplayOrder IS NULL THEN 999
                ELSE pm.DisplayOrder
            END,
            pm.MediaID
    ) cover
    {whereClause}
    ORDER BY
        p.CreatedAt DESC,
        p.PropertyID DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
    """;
        parameters.Add(
            "Offset",
            offset);

        parameters.Add(
            "PageSize",
            pageSize);

        IEnumerable<AdminPropertyResponse> properties =
            await connection.QueryAsync<AdminPropertyResponse>(
                dataSql,
                parameters);

        List<AdminPropertyResponse> propertyList =
            properties.ToList();

        int totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

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
    // Retrives Property Whose Status is Set to Pending and also also adds pagination only loads 3 property cards per page
    public async Task<AdminPendingApprovalListResponse>
    GetPendingApprovalsAsync(
        int pageNumber)
    {
        const int pageSize = 3;

        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        int offset =
            (pageNumber - 1) * pageSize;

        using var connection =
            _connectionFactory.CreateConnection();

        const string countSql = """
        SELECT COUNT(1)
        FROM Properties
        WHERE ListingStatus = 'Pending';
        """;

        int totalRecords =
            await connection.ExecuteScalarAsync<int>(
                countSql);

        string dataSql = """
        SELECT
            p.PropertyID,
            p.PropertyGUID,
            p.SellerID,
            u.FullName AS SellerName,
            p.PropertyTitle,
            p.PropertyType,
            p.ListingType,
            p.LocationAddress,
            p.City,
            p.State,
            p.Price,
            COALESCE(
                p.UpdatedAt,
                p.CreatedAt
            ) AS SubmittedAt,
            cover.FilePath AS CoverImagePath
        FROM Properties p
        INNER JOIN Users u
            ON p.SellerID = u.UserID
        OUTER APPLY
        (
            SELECT TOP 1
                pm.FilePath
            FROM PropertyMedia pm
            WHERE pm.PropertyID = p.PropertyID
              AND pm.MediaType = 'CoverPhoto'
            ORDER BY
                CASE
                    WHEN pm.DisplayOrder IS NULL
                    THEN 999
                    ELSE pm.DisplayOrder
                END,
                pm.MediaID
        ) cover
        WHERE p.ListingStatus = 'Pending'
        ORDER BY
            COALESCE(
                p.UpdatedAt,
                p.CreatedAt
            ) DESC,
            p.PropertyID DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

        IEnumerable<AdminPendingApprovalResponse> properties =
            await connection.QueryAsync<AdminPendingApprovalResponse>(
                dataSql,
                new
                {
                    Offset = offset,
                    PageSize = pageSize
                });

        int totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        return new AdminPendingApprovalListResponse
        {
            Properties = properties.ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }
    public async Task<bool>
    UpdatePendingPropertyStatusAsync(
        int propertyId,
        string listingStatus)
    {
        const string query = """
        UPDATE Properties
        SET
            ListingStatus = @ListingStatus,
            UpdatedAt = GETUTCDATE()
        WHERE PropertyID = @PropertyID
          AND ListingStatus = 'Pending';
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        int rowsAffected =
            await connection.ExecuteAsync(
                query,
                new
                {
                    PropertyID = propertyId,
                    ListingStatus = listingStatus
                });

        return rowsAffected > 0;
    }
    public async Task<int> GetApprovedPropertyCountAsync()
    {
        const string sql = """
    SELECT COUNT(1)
    FROM Properties
    WHERE ListingStatus = 'Approved';
    """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(sql);
    }
    public async Task<IEnumerable<PublicPropertyResponse>>
     GetLatestApprovedPropertiesAsync(
         string? city,
         int count)
    {
        const string sql = """
    SELECT TOP (@Count)
        p.PropertyID,
        p.PropertyTitle,
        p.PropertyType,
        p.ListingType,
        p.City,
        p.State,
        p.LocationAddress,
        p.Price,
        p.Bedrooms,
        p.Bathrooms,
        p.Area,
        p.AreaUnit,
        cover.FilePath AS CoverImagePath
    FROM Properties p
    OUTER APPLY
    (
        SELECT TOP 1
            pm.FilePath
        FROM PropertyMedia pm
        WHERE pm.PropertyID = p.PropertyID
          AND pm.MediaType = 'CoverPhoto'
        ORDER BY
            CASE
                WHEN pm.DisplayOrder IS NULL THEN 999
                ELSE pm.DisplayOrder
            END,
            pm.MediaID
    ) cover
    WHERE p.ListingStatus = 'Approved'
      AND
      (
          @City IS NULL
          OR p.City =
              CASE @City
                  WHEN 'Bengaluru'
                      THEN 'Bangalore Urban'

                  WHEN 'Mumbai'
                      THEN 'Mumbai'

                  WHEN 'Kolkata'
                      THEN 'Kolkata'

                  WHEN 'Chennai'
                      THEN 'Chennai'

                  WHEN 'Delhi'
                      THEN 'Delhi'

                  ELSE @City
              END
      )
    ORDER BY
        p.CreatedAt DESC,
        p.PropertyID DESC;
    """;

        using var connection =
            _connectionFactory.CreateConnection();

        return await connection.QueryAsync<PublicPropertyResponse>(
            sql,
            new
            {
                City = string.IsNullOrWhiteSpace(city)
                    ? null
                    : city.Trim(),

                Count = count
            });
    }
    /// <summary>
    /// Retrieves approved properties matching the supplied Buyer search criteria.
    /// </summary>
    /// <remarks>
    /// Data flow:
    /// PropertyService
    ///     → IPropertyRepository
    ///     → SQL Server
    ///
    /// The query always restricts results to properties whose ListingStatus is Approved.
    /// Pagination is applied at the database level with a fixed page size of six.
    /// </remarks>
    public async Task<BuyerPropertyListResponse>
    SearchApprovedPropertiesAsync(
        BuyerPropertySearchRequest request,
        int buyerId,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 6;

        int pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        int offset =
            (pageNumber - 1) * pageSize;

        string? search =
            string.IsNullOrWhiteSpace(request.Search)
                ? null
                : request.Search.Trim();

        string? locationType =
            string.IsNullOrWhiteSpace(request.LocationType)
                ? null
                : request.LocationType.Trim();

        string? locationValue =
            string.IsNullOrWhiteSpace(request.LocationValue)
                ? null
                : request.LocationValue.Trim();

        string sortBy =
            string.IsNullOrWhiteSpace(request.SortBy)
                ? "latest"
                : request.SortBy.Trim().ToLowerInvariant();

        List<string> listingTypes =
            request.ListingTypes
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

        List<string> listingTypesForSql =
            listingTypes.Count == 0
                ? ["__NO_LISTING_TYPE__"]
                : listingTypes;

        DynamicParameters parameters =
            new DynamicParameters();
        parameters.Add(
               "UserID",
          buyerId);

        parameters.Add(
            "SearchPattern",
            search == null ? null : $"{search}%");

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
            listingTypesForSql);

        parameters.Add(
            "SortBy",
            sortBy);

        parameters.Add(
            "Offset",
            offset);

        parameters.Add(
            "PageSize",
            pageSize);

        const string whereClause = """
        WHERE p.ListingStatus = 'Approved'

          AND
          (
              @SearchPattern IS NULL
              OR p.Country LIKE @SearchPattern
              OR p.State LIKE @SearchPattern
              OR p.City LIKE @SearchPattern
          )

          AND
          (
              @LocationValue IS NULL

              OR
              (
                  @LocationType = 'State'
                  AND p.State = @LocationValue
              )

              OR
              (
                  @LocationType = 'City'
                  AND
                  (
                      p.City = @LocationValue

                      OR
                      (
                          @LocationValue = 'Bengaluru'
                          AND p.City = 'Bangalore Urban'
                      )
                  )
              )
          )

          AND
          (
              @MinPrice IS NULL
              OR p.Price >= @MinPrice
          )

          AND
          (
              @MaxPrice IS NULL
              OR p.Price <= @MaxPrice
          )

          AND
          (
              @PropertyType IS NULL
              OR p.PropertyType = @PropertyType
          )

          AND
          (
              @MinBedrooms IS NULL
              OR
              TRY_CONVERT(int, p.Bedrooms) >= @MinBedrooms
          )

          AND
          (
              @HasListingTypeFilter = 0
              OR p.ListingType IN @ListingTypes
          )
        """;

        const string countSql = $"""
        SELECT COUNT(1)
        FROM Properties p
        {whereClause};
        """;

        const string dataSql = $"""
        SELECT
        p.PropertyID,
        p.PropertyGUID,
        p.PropertyTitle,
        p.PropertyType,
        p.ListingType,
        p.LocationAddress,
        p.Country,
        p.State,
        p.City,
        p.ZipCode,
        p.Price,
        p.Area,
        p.AreaUnit,
        p.Bedrooms,
        p.Bathrooms,
        cover.FilePath AS CoverImagePath,

        CASE
            WHEN EXISTS
            (
                SELECT 1
                FROM BuyerFavorites bf
                WHERE bf.UserID = @UserID
                  AND bf.PropertyID = p.PropertyID
            )
            THEN CAST(1 AS BIT)
            ELSE CAST(0 AS BIT)
        END AS IsFavorite
        FROM Properties p
        OUTER APPLY
        (
            SELECT TOP 1
                pm.FilePath
            FROM PropertyMedia pm
            WHERE pm.PropertyID = p.PropertyID
              AND pm.MediaType = 'CoverPhoto'
            ORDER BY
                CASE
                    WHEN pm.DisplayOrder IS NULL
                    THEN 999
                    ELSE pm.DisplayOrder
                END,
                pm.MediaID
        ) cover
        {whereClause}
        ORDER BY
            CASE
                WHEN @SortBy = 'latest'
                THEN COALESCE(p.UpdatedAt, p.CreatedAt)
            END DESC,

            CASE
                WHEN @SortBy = 'price-low'
                THEN p.Price
            END ASC,

            CASE
                WHEN @SortBy = 'price-high'
                THEN p.Price
            END DESC,

            p.PropertyID DESC
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        """;

        using var connection =
            _connectionFactory.CreateConnection();

        CommandDefinition command =
            new(
                $"{countSql}\n{dataSql}",
                parameters,
                cancellationToken: cancellationToken);

        using SqlMapper.GridReader grid =
            await connection.QueryMultipleAsync(command);

        int totalRecords =
            await grid.ReadSingleAsync<int>();

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

        List<int> propertyIds =
            propertyRows
                .Select(x => x.PropertyID)
                .ToList();

        DynamicParameters detailParameters =
            new DynamicParameters();

        detailParameters.Add(
            "PropertyIds",
            propertyIds);

        const string amenitiesSql = """
        SELECT
            pa.PropertyID,
            a.AmenityID,
            a.AmenityName,
            a.Category
        FROM PropertyAmenities pa
        INNER JOIN Amenities a
            ON pa.AmenityID = a.AmenityID
        WHERE pa.PropertyID IN @PropertyIds
        ORDER BY
            pa.PropertyID,
            a.Category,
            a.AmenityName;
        """;

        const string mediaSql = """
        SELECT
            MediaID,
            PropertyID,
            MediaType,
            FileName,
            FilePath,
            DisplayOrder
        FROM PropertyMedia
        WHERE PropertyID IN @PropertyIds
          AND MediaType IN
          (
              'CoverPhoto',
              'GalleryImage'
          )
        ORDER BY
            PropertyID,
            CASE
                WHEN MediaType = 'CoverPhoto' THEN 1
                ELSE 2
            END,
            CASE
                WHEN DisplayOrder IS NULL THEN 999
                ELSE DisplayOrder
            END,
            MediaID;
        """;

        CommandDefinition detailCommand =
            new(
                $"{amenitiesSql}\n{mediaSql}",
                detailParameters,
                cancellationToken: cancellationToken);

        using SqlMapper.GridReader detailGrid =
            await connection.QueryMultipleAsync(
                detailCommand);

        List<BuyerPropertyAmenityRow> amenityRows =
            (
                await detailGrid
                    .ReadAsync<BuyerPropertyAmenityRow>()
            ).ToList();

        List<BuyerPropertyMediaRow> mediaRows =
            (
                await detailGrid
                    .ReadAsync<BuyerPropertyMediaRow>()
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

            Amenities =
                amenitiesByProperty.TryGetValue(
                    row.PropertyID,
                    out List<BuyerPropertyAmenityDto>? amenities)
                    ? amenities
                    : [],

            Media =
                mediaByProperty.TryGetValue(
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

    private static int CalculateTotalPages(
        int totalRecords,
        int pageSize)
    {
        return totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);
    }

    private sealed class BuyerPropertyCardRow
    {
        public int PropertyID { get; set; }
        public Guid PropertyGUID
        {
            get; set;
        }
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

    private sealed class BuyerPropertyAmenityRow
    {
        public int PropertyID { get; set; }

        public int AmenityID { get; set; }

        public string AmenityName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }

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