namespace Brokerage.Web.Models.ApiResponses
{
    public class LoginApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
