using Brokerage.Web.Models.ApiModels;

namespace Brokerage.Web.Models.ApiResponses;

public class AdminPendingApprovalListApiResponse
{
    public List<AdminPendingApprovalApiModel> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}