using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiRequests;
using Brokerage.Web.Models.ApiResponses;
using System.Net.Http.Headers;

namespace Brokerage.Web.Services;

public class PropertyApiClient
{
    private readonly HttpClient _httpClient;

    public PropertyApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient(
                "BrokerageApi");
    }



    public async Task<CreatePropertyApiResponse>
        CreatePropertyAsync(
            CreatePropertyApiRequest request,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                "api/Property/create",
                request,
                cancellationToken);

        return await ReadResponseAsync<CreatePropertyApiResponse>(
            response,
            cancellationToken);
    }


    public async Task<List<PropertyApiModel>>
        GetMyPropertiesAsync(
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/Property/my-properties",
                cancellationToken);

        return await ReadResponseAsync<List<PropertyApiModel>>(
            response,
            cancellationToken);
    }



    public async Task<PropertyApiModel?>
        GetPropertyByIdAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/{propertyId}",
                cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadResponseAsync<PropertyApiModel>(
            response,
            cancellationToken);
    }

    public async Task<PropertyApiModel?>
        GetPropertyByGuidAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/{propertyGuid}",
                cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadResponseAsync<PropertyApiModel>(
            response,
            cancellationToken);
    }



    public async Task<ApiMessageResponse>
        UpdatePropertyAsync(
            int propertyId,
            UpdatePropertyApiRequest request,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync(
                $"api/Property/{propertyId}",
                request,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        UpdatePropertyAsync(
            Guid propertyGuid,
            UpdatePropertyApiRequest request,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync(
                $"api/Property/{propertyGuid}",
                request,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }



    public async Task<SubmitPropertyApiResponse>
        SubmitPropertyAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PutAsync(
                $"api/Property/{propertyId}/submit",
                null,
                cancellationToken);

        return await ReadResponseAsync<SubmitPropertyApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<SubmitPropertyApiResponse>
        SubmitPropertyAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PutAsync(
                $"api/Property/{propertyGuid}/submit",
                null,
                cancellationToken);

        return await ReadResponseAsync<SubmitPropertyApiResponse>(
            response,
            cancellationToken);
    }



    public async Task<List<AmenityApiModel>>
        GetAllAmenitiesAsync(
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/Property/amenities",
                cancellationToken);

        return await ReadResponseAsync<List<AmenityApiModel>>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        SavePropertyAmenitiesAsync(
            int propertyId,
            SavePropertyAmenitiesApiRequest request,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                $"api/Property/{propertyId}/amenities",
                request,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        SavePropertyAmenitiesAsync(
            Guid propertyGuid,
            SavePropertyAmenitiesApiRequest request,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                $"api/Property/{propertyGuid}/amenities",
                request,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }


    public async Task<List<PropertyMediaApiModel>>
        GetPropertyMediaAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/{propertyId}/media",
                cancellationToken);

        return await ReadResponseAsync<List<PropertyMediaApiModel>>(
            response,
            cancellationToken);
    }

    public async Task<List<PropertyMediaApiModel>>
        GetPropertyMediaAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/{propertyGuid}/media",
                cancellationToken);

        return await ReadResponseAsync<List<PropertyMediaApiModel>>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        UploadPropertyMediaAsync(
            int propertyId,
            IFormFile? coverPhoto,
            IEnumerable<IFormFile> galleryImages,
            IEnumerable<IFormFile> videos,
            IEnumerable<IFormFile> floorPlans,
            IEnumerable<IFormFile> documents,
            CancellationToken cancellationToken = default)
    {
        using MultipartFormDataContent content =
            BuildMultipartContent(coverPhoto, galleryImages, videos, floorPlans, documents);

        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/Property/{propertyId}/media",
                content,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        UploadPropertyMediaAsync(
            Guid propertyGuid,
            IFormFile? coverPhoto,
            IEnumerable<IFormFile> galleryImages,
            IEnumerable<IFormFile> videos,
            IEnumerable<IFormFile> floorPlans,
            IEnumerable<IFormFile> documents,
            CancellationToken cancellationToken = default)
    {
        using MultipartFormDataContent content =
            BuildMultipartContent(coverPhoto, galleryImages, videos, floorPlans, documents);

        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/Property/{propertyGuid}/media",
                content,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }


    public async Task<PropertyReviewApiResponse?>
        GetPropertyReviewAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/{propertyId}/review",
                cancellationToken);

        return await ReadResponseAsync<PropertyReviewApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<PropertyReviewApiResponse?>
        GetPropertyReviewByGuidAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/{propertyGuid}/review",
                cancellationToken);

        return await ReadResponseAsync<PropertyReviewApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<PropertyReviewApiResponse?>
        GetPropertyReviewAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        return await GetPropertyReviewByGuidAsync(propertyGuid, cancellationToken);
    }

    public async Task<HttpResponseMessage> GetPropertyImageAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync(
            path,
            cancellationToken);
    }



    public async Task<BuyerPropertyListApiResponse>
        SearchBuyerPropertiesAsync(
            BuyerPropertySearchRequest request,
            CancellationToken cancellationToken = default)
    {
        List<string> queryParameters = [];

        queryParameters.Add($"pageNumber={request.PageNumber}");
        AddQueryParameter(queryParameters, "search", request.Search);
        AddQueryParameter(queryParameters, "locationType", request.LocationType);
        AddQueryParameter(queryParameters, "locationValue", request.LocationValue);

        if (request.MinPrice.HasValue)
            queryParameters.Add($"minPrice={request.MinPrice.Value}");

        if (request.MaxPrice.HasValue)
            queryParameters.Add($"maxPrice={request.MaxPrice.Value}");

        AddQueryParameter(queryParameters, "propertyType", request.PropertyType);

        if (request.MinBedrooms.HasValue)
            queryParameters.Add($"minBedrooms={request.MinBedrooms.Value}");

        foreach (string listingType in request.ListingTypes)
        {
            queryParameters.Add($"listingTypes={Uri.EscapeDataString(listingType)}");
        }

        AddQueryParameter(queryParameters, "sortBy", request.SortBy);

        string queryString = string.Join("&", queryParameters);

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Property/buyer-listings?{queryString}",
                cancellationToken);

        return await ReadResponseAsync<BuyerPropertyListApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<List<LocationSearchOptionApiModel>>
        SearchIndiaLocationsAsync(
            string? search,
            CancellationToken cancellationToken = default)
    {
        string query = string.IsNullOrWhiteSpace(search)
            ? string.Empty
            : $"?search={Uri.EscapeDataString(search.Trim())}";

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/locations/india{query}",
                cancellationToken);

        return await ReadResponseAsync<List<LocationSearchOptionApiModel>>(
            response,
            cancellationToken);
    }



    public async Task<ApiMessageResponse>
        AddFavoriteAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/favorites/{propertyGuid}",
                null,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        AddFavoriteAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/favorites/{propertyId}",
                null,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        RemoveFavoriteAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.DeleteAsync(
                $"api/favorites/{propertyGuid}",
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        RemoveFavoriteAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.DeleteAsync(
                $"api/favorites/{propertyId}",
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<FavoriteStatusApiResponse>
        IsFavoriteAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/favorites/{propertyGuid}/exists",
                cancellationToken);

        return await ReadResponseAsync<FavoriteStatusApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<FavoriteStatusApiResponse>
        IsFavoriteAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/favorites/{propertyId}/exists",
                cancellationToken);

        return await ReadResponseAsync<FavoriteStatusApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<BuyerPropertyListApiResponse>
        GetFavoritesAsync(
            int pageNumber,
            CancellationToken cancellationToken = default)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/favorites?pageNumber={pageNumber}&pageSize=6",
                cancellationToken);

        return await ReadResponseAsync<BuyerPropertyListApiResponse>(
            response,
            cancellationToken);
    }



    public async Task<CreateBuyRequestApiResponse>
        CreateBuyRequestAsync(
            Guid propertyGuid,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/buyrequests/{propertyGuid}",
                null,
                cancellationToken);

        return await ReadResponseAsync<CreateBuyRequestApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<CreateBuyRequestApiResponse>
        CreateBuyRequestAsync(
            int propertyId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/buyrequests/{propertyId}",
                null,
                cancellationToken);

        return await ReadResponseAsync<CreateBuyRequestApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<List<SellerBuyRequestApiModel>>
        GetSellerBuyRequestsAsync(
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/buyrequests/seller",
                cancellationToken);

        return await ReadResponseAsync<List<SellerBuyRequestApiModel>>(
            response,
            cancellationToken);
    }

    public async Task<ApiMessageResponse>
        RejectBuyRequestAsync(
            int buyRequestId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/buyrequests/{buyRequestId}/reject",
                null,
                cancellationToken);

        return await ReadResponseAsync<ApiMessageResponse>(
            response,
            cancellationToken);
    }

    public async Task<ApproveBuyRequestApiResponse>
        ApproveBuyRequestAsync(
            int buyRequestId,
            CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.PostAsync(
                $"api/buyrequests/{buyRequestId}/approve",
                null,
                cancellationToken);

        return await ReadResponseAsync<ApproveBuyRequestApiResponse>(
            response,
            cancellationToken);
    }



    private static MultipartFormDataContent BuildMultipartContent(
        IFormFile? coverPhoto,
        IEnumerable<IFormFile> galleryImages,
        IEnumerable<IFormFile> videos,
        IEnumerable<IFormFile> floorPlans,
        IEnumerable<IFormFile> documents)
    {
        MultipartFormDataContent content = new();

        if (coverPhoto != null)
        {
            content.Add(CreateFileContent(coverPhoto), "CoverPhoto", coverPhoto.FileName);
        }

        foreach (IFormFile file in galleryImages)
        {
            content.Add(CreateFileContent(file), "GalleryImages", file.FileName);
        }

        foreach (IFormFile file in videos)
        {
            content.Add(CreateFileContent(file), "Videos", file.FileName);
        }

        foreach (IFormFile file in floorPlans)
        {
            content.Add(CreateFileContent(file), "FloorPlans", file.FileName);
        }

        foreach (IFormFile file in documents)
        {
            content.Add(CreateFileContent(file), "Documents", file.FileName);
        }

        return content;
    }

    private static StreamContent CreateFileContent(IFormFile file)
    {
        StreamContent content = new(file.OpenReadStream());

        content.Headers.ContentType = new MediaTypeHeaderValue(
            file.ContentType ?? "application/octet-stream");

        return content;
    }

    private static void AddQueryParameter(
        List<string> queryParameters,
        string key,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            queryParameters.Add(
                $"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static async Task<T> ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(cancellationToken);

            throw new HttpRequestException(
                $"API request failed. Status: {(int)response.StatusCode}. Response: {error}",
                null,
                response.StatusCode);
        }

        T? result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException("API returned an empty response.");
        }

        return result;
    }
}