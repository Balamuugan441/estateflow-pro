namespace Brokerage.Web.Models.ApiResponses;

public class AdminActivityLogListApiResponse
{
    public List<AdminActivityLogApiModel> Logs { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}