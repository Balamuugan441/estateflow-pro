namespace Brokerage.Web.Models.ApiResponses;

public class SubmitPropertyApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<string> Errors { get; set; } = [];
}