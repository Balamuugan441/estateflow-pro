namespace Brokerage.Web.Models.ApiModels;

public class AdminActivityLogQueryApiRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ActorSearch { get; set; }
    public string? ActionCategory { get; set; }
    public int PageNumber { get; set; } = 1;
}