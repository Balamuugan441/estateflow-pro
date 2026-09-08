
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Authentication;
using Brokerage.Models.Entities;

namespace Brokerage.Business.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request)
    {

        if (request.Role != "Buyer" &&
            request.Role != "Seller")
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Invalid registration role."
            };
        }


        // Check whether email already exists.
        bool emailExists =
            await _userRepository.EmailExistsAsync(request.Email);

        if (emailExists)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "An account with this email already exists."
            };
        }


        // Find the RoleID from the Roles table.
        int? roleId =
            await _userRepository.GetRoleIdByNameAsync(request.Role);

        if (roleId == null)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Selected role does not exist."
            };
        }


        string passwordHash =
            BCrypt.Net.BCrypt.HashPassword(
                request.Password,
                workFactor: 12);

        User user = new User
        {
            RoleID = roleId.Value,
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            MobileNumber = request.MobileNumber
        };

        await _userRepository.CreateUserAsync(user);


        return new RegisterResponse
        {
            Success = true,
            Message = "Registration successful."
        };
    }
    public async Task<LoginResponse>LoginAsync(LoginRequest request)
    {
        User? user =
         await _userRepository.GetUserByEmailAsync(
             request.Email);


        if (user == null)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }


        if (!user.IsActive)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "The Account is Currently not Active"
            };
        }

        bool passwordMatches =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);


        if (!passwordMatches)
        {
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email
        };

    }
}