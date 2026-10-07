namespace Brokerage.Models.DTOs.Admin;

public class AdminPropertyListResponse
{
    public List<AdminPropertyResponse> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public int TotalLive { get; set; }

    public int TotalPending { get; set; }

    public int TotalRejected { get; set; }
}