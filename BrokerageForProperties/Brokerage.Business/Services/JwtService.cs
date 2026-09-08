using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Brokerage.Business.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(int userId, string fullName, string email, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            string secretKey =
                jwtSettings["SecretKey"]!;

            string issuer =
                jwtSettings["Issuer"]!;

            string audience =
                jwtSettings["Audience"]!;

            int expirationMinutes =
                int.Parse(jwtSettings["ExpirationMinutes"]!);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,userId.ToString()),
                new Claim(ClaimTypes.Name,fullName),
                new Claim(ClaimTypes.Email,email),
                new Claim(ClaimTypes.Role,role)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                    signingCredentials: credentials);

            return new JwtSecurityTokenHandler()
                .WriteToken(token);

        }
    }
}
