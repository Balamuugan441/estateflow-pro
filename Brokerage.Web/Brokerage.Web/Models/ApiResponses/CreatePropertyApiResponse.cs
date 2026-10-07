namespace Brokerage.Web.Models.ApiResponses;

public class CreatePropertyApiResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int PropertyID { get; set; }
}