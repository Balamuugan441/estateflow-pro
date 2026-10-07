namespace Brokerage.Models.DTOs.Admin;

public class AdminUserQueryRequest
{
    public string? Search { get; set; }

    public string? RoleName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int PageNumber { get; set; } = 1;
}