using Brokerage.Web.Models.ApiModels;
using Brokerage.Web.Models.ApiResponses;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;

namespace Brokerage.Web.Services;

public class AdminApiClient
{
    private readonly HttpClient _httpClient;

    public AdminApiClient(
        IHttpClientFactory httpClientFactory)
    {
        _httpClient =
            httpClientFactory.CreateClient(
                "BrokerageApi");
    }

    public async Task<AdminUserListApiResponse>
        GetUsersAsync(
            string? search,
            string? roleName,
            bool? isActive,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            CancellationToken cancellationToken = default)
    {
        Dictionary<string, string?> query =
            new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query["search"] = search;
        }

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            query["roleName"] = roleName;
        }

        if (isActive.HasValue)
        {
            query["isActive"] =
                isActive.Value.ToString()
                    .ToLowerInvariant();
        }

        if (fromDate.HasValue)
        {
            query["fromDate"] =
                fromDate.Value.ToString(
                    "yyyy-MM-dd");
        }

        if (toDate.HasValue)
        {
            query["toDate"] =
                toDate.Value.ToString(
                    "yyyy-MM-dd");
        }

        query["pageNumber"] =
            pageNumber.ToString();

        string url =
            QueryHelpers.AddQueryString(
                "api/Admin/users",
                query);

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<AdminUserListApiResponse>(
                cancellationToken)
            ?? new AdminUserListApiResponse();
    }
    public async Task<AdminPropertyListApiResponse>
    GetPropertiesAsync(
        string? search,
        string? listingStatus,
        string? propertyType,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, string?> query = [];

        if (!string.IsNullOrWhiteSpace(search))
        {
            query["search"] = search;
        }

        if (!string.IsNullOrWhiteSpace(listingStatus))
        {
            query["listingStatus"] = listingStatus;
        }

        if (!string.IsNullOrWhiteSpace(propertyType))
        {
            query["propertyType"] = propertyType;
        }

        if (minPrice.HasValue)
        {
            query["minPrice"] =
                minPrice.Value.ToString(
                    CultureInfo.InvariantCulture);
        }

        if (maxPrice.HasValue)
        {
            query["maxPrice"] =
                maxPrice.Value.ToString(
                    CultureInfo.InvariantCulture);
        }

        query["pageNumber"] =
            pageNumber.ToString();

        string url =
            QueryHelpers.AddQueryString(
                "api/Admin/properties",
                query);

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<AdminPropertyListApiResponse>(
                cancellationToken)
            ?? new AdminPropertyListApiResponse();
    }
    public async Task<AdminPendingApprovalListApiResponse>
     GetPendingApprovalsAsync(
         int pageNumber,
         CancellationToken cancellationToken = default)
    {
        string url =
            $"api/Admin/pending-approvals?pageNumber={pageNumber}";

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                url,
                cancellationToken);

        return await ReadResponseAsync<
            AdminPendingApprovalListApiResponse>(
            response,
            cancellationToken);
    }

    public async Task<AdminPropertyActionApiResponse>
        UpdatePendingPropertyStatusAsync(
            int propertyId,
            string listingStatus,
            CancellationToken cancellationToken = default)
    {
        object request = new
        {
            ListingStatus = listingStatus
        };

        HttpResponseMessage response =
            await _httpClient.PutAsJsonAsync(
                $"api/Admin/pending-approvals/{propertyId}/status",
                request,
                cancellationToken);

        return await ReadResponseAsync<
            AdminPropertyActionApiResponse>(
            response,
            cancellationToken);
    }
    private static async Task<T>
    ReadResponseAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            string error =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new HttpRequestException(
                $"API request failed. " +
                $"Status: {(int)response.StatusCode}. " +
                $"Response: {error}",
                null,
                response.StatusCode);
        }

        T? result =
            await response.Content.ReadFromJsonAsync<T>(
                cancellationToken: cancellationToken);

        if (result == null)
        {
            throw new InvalidOperationException(
                "API returned an empty response.");
        }

        return result;
    }
    public async Task<PropertyReviewApiResponse?>
    GetPropertyPreviewAsync(
        int propertyId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Admin/pending-approvals/{propertyId}/preview",
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ReadResponseAsync<
            PropertyReviewApiResponse>(
            response,
            cancellationToken);
    }
    public async Task<AdminDashboardApiResponse>
    GetDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response =
            await _httpClient.GetAsync(
                "api/Admin/dashboard",
                cancellationToken);

        return await ReadResponseAsync<
            AdminDashboardApiResponse>(
            response,
            cancellationToken);
    }
    public async Task<AdminActivityLogListApiResponse>
    GetActivityLogsAsync(
        AdminActivityLogQueryApiRequest request,
        CancellationToken cancellationToken = default)
    {
        List<string> queryParameters = [];

        if (request.FromDate.HasValue)
        {
            queryParameters.Add(
                $"FromDate={Uri.EscapeDataString(
                    request.FromDate.Value.ToString("yyyy-MM-dd"))}");
        }

        if (request.ToDate.HasValue)
        {
            queryParameters.Add(
                $"ToDate={Uri.EscapeDataString(
                    request.ToDate.Value.ToString("yyyy-MM-dd"))}");
        }

        if (!string.IsNullOrWhiteSpace(request.ActorSearch))
        {
            queryParameters.Add(
                $"ActorSearch={Uri.EscapeDataString(
                    request.ActorSearch.Trim())}");
        }

        if (!string.IsNullOrWhiteSpace(request.ActionCategory))
        {
            queryParameters.Add(
                $"ActionCategory={Uri.EscapeDataString(
                    request.ActionCategory)}");
        }

        queryParameters.Add(
            $"PageNumber={request.PageNumber}");

        string queryString =
            string.Join("&", queryParameters);

        HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"api/Admin/activity-logs?{queryString}",
                cancellationToken);

        return await ReadResponseAsync<AdminActivityLogListApiResponse>(
            response,
            cancellationToken);
    }
    // Sends the requested user status to the Admin API through the existing BFF HttpClient.
    public async Task<AdminUserStatusApiResponse> UpdateUserStatusAsync(int userId, bool isActive, CancellationToken cancellationToken = default)

    {
        object request = new
        {
            IsActive = isActive
        };

        HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/Admin/users/{userId}/status", request, cancellationToken);

        return await ReadResponseAsync<AdminUserStatusApiResponse>(response, cancellationToken);

    }

}