using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Home;

namespace Brokerage.Business.Services;

public class HomeService
{
    private readonly IUserRepository _userRepository;
    private readonly IPropertyRepository _propertyRepository;

    public HomeService(
        IUserRepository userRepository,
        IPropertyRepository propertyRepository)
    {
        _userRepository = userRepository;
        _propertyRepository = propertyRepository;
    }

    public async Task<PublicHomeStatsResponse>
        GetStatsAsync()
    {
        int totalUsers =
            await _userRepository.GetTotalUserCountAsync();

        int approvedProperties =
            await _propertyRepository
                .GetApprovedPropertyCountAsync();

        return new PublicHomeStatsResponse
        {
            TotalUsers = totalUsers,
            ApprovedProperties = approvedProperties
        };
    }

    public async Task<IEnumerable<PublicPropertyResponse>>
        GetLatestPropertiesAsync(
            string? city)
    {
        const int propertyCount = 3;

        return await _propertyRepository
            .GetLatestApprovedPropertiesAsync(
                city,
                propertyCount);
    }
}