namespace Brokerage.Models.DTOs.Admin;

public class AdminPendingApprovalListResponse
{
    public List<AdminPendingApprovalResponse> Properties { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}