using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Models.ApiResponses;

public class AdminPropertyListApiResponse
{
    public List<AdminPropertyApiModel> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public int TotalLive { get; set; }

    public int TotalPending { get; set; }

    public int TotalRejected { get; set; }
}