
using Brokerage.Data.Interfaces;
using Brokerage.Models.DTOs.Authentication;
using Brokerage.Models.Entities;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Brokerage.Business.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ActivityLogService _activityLogService;

    public AuthService(IUserRepository userRepository, JwtService jwtService, IEmailService emailService, IConfiguration configuration, ActivityLogService activityLogService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _emailService = emailService;
        _configuration = configuration;
        _activityLogService = activityLogService;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request)
    {

        if (request.Role != "Buyer" &&
            request.Role != "Seller")
        {
            await _activityLogService.LogAsync(
            null,
            request.FullName,
            null,
            "USER_REGISTRATION_FAILED",
            "USER",
            "USER",
            null,
            request.Email,
            "Failed",
            "Invalid registration role.");
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
            await _activityLogService.LogAsync(
        null,
        request.FullName,
        null,
        "USER_REGISTRATION_FAILED",
        "USER",
        "USER",
        null,
        request.Email,
        "Failed",
        "Registration attempted with an existing email.");
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
            await _activityLogService.LogAsync(
        null,
        request.FullName,
        request.Role,
        "USER_REGISTRATION_FAILED",
        "USER",
        "USER",
        null,
        request.Email,
        "Failed",
        "Selected role does not exist.");
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

        int userId =
    await _userRepository.CreateUserAsync(user);
        await _activityLogService.LogAsync(
    userId,
    request.FullName,
    request.Role,
    "USER_REGISTERED",
    "USER",
    "USER",
    userId,
    request.FullName,
    "Success",
    "New user account was created.");


        return new RegisterResponse
        {
            Success = true,
            Message = "Registration successful."
        };
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        User? user =
         await _userRepository.GetUserByEmailAsync(
             request.Email);


        if (user == null)
        {
            await _activityLogService.LogAsync(
        null,
        "System",
        null,
        "USER_LOGIN_FAILED",
        "AUTHENTICATION",
        "USER",
        null,
        request.Email,
        "Failed",
        "Login failed because the user was not found.");

            return new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }


        if (!user.IsActive)
        {
            await _activityLogService.LogAsync(
       user.UserID,
       user.FullName,
       user.RoleName,
       "USER_LOGIN_BLOCKED",
       "AUTHENTICATION",
       "USER",
       user.UserID,
       user.FullName,
       "Failed",
       "Login was blocked because the account is inactive.");
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
            await _activityLogService.LogAsync(
       user.UserID,
       user.FullName,
       user.RoleName,
       "USER_LOGIN_FAILED",
       "AUTHENTICATION",
       "USER",
       user.UserID,
       user.FullName,
       "Failed",
       "Login failed because the password was incorrect.");
            return new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }
        string token =
    _jwtService.GenerateToken(
        user.UserID,
        user.FullName,
        user.Email,
        user.RoleName);
        await _activityLogService.LogAsync(
    user.UserID,
    user.FullName,
    user.RoleName,
    "USER_LOGIN_SUCCESS",
    "AUTHENTICATION",
    "USER",
    user.UserID,
    user.FullName,
    "Success",
    "User logged in successfully.");

        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserID,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.RoleName,
            Token = token
        };

    }
    //Forgot Password Login Page
    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(
    ForgotPasswordRequest request)
    {
        User? user =
            await _userRepository.GetUserByEmailAsync(
                request.Email);

        if (user == null)
        {
            return new ForgotPasswordResponse
            {
                Success = true,
                Message =
                    "If an account exists for this email address, a password reset link has been sent."
            };
        }

        await _userRepository
            .InvalidateExistingPasswordResetTokensAsync(
                user.UserID);

        string resetToken =
            Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator
                    .GetBytes(32));

        string tokenHash =
            Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(resetToken)));

        var passwordResetToken =
            new PasswordResetToken
            {
                UserID = user.UserID,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

        await _userRepository.CreatePasswordResetTokenAsync(
            passwordResetToken);

        string webBaseUrl =
     _configuration["WebSettings:BaseUrl"]!;

        string resetLink =
            $"{webBaseUrl}/Account/ResetPassword?token={Uri.EscapeDataString(resetToken)}";

        string emailBody = $"""
        <h2>EstateFlow Pro</h2>

        <p>Hello {user.FullName},</p>

        <p>
            We received a request to reset your EstateFlow Pro password.
        </p>

        <p>
            Click the button below to create a new password.
        </p>

        <p>
            <a href="{resetLink}">
                Reset Password
            </a>
        </p>

        <p>
            This link will expire in 30 minutes.
        </p>

        <p>
            If you did not request a password reset, you can safely ignore this email.
        </p>

        <p>
            Regards,<br />
            EstateFlow Pro Team
        </p>
        """;

        await _emailService.SendAsync(
            user.Email,
            "EstateFlow Pro - Reset Your Password",
            emailBody);
        await _activityLogService.LogAsync(
    user.UserID,
    user.FullName,
    user.RoleName,
    "PASSWORD_RESET_REQUESTED",
    "AUTHENTICATION",
    "USER",
    user.UserID,
    user.FullName,
    "Success",
    "Password reset link was requested.");

        return new ForgotPasswordResponse
        {
            Success = true,
            Message =
                "If an account exists for this email address, a password reset link has been sent."
        };
    }
    public async Task<ResetPasswordResponse> ResetPasswordAsync(
    ResetPasswordRequest request)
    {
        string tokenHash =
            Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(request.Token)));

        PasswordResetToken? resetToken =
            await _userRepository.GetPasswordResetTokenAsync(
                tokenHash);

        if (resetToken == null)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "Invalid or expired password reset link."
            };
        }

        if (resetToken.UsedAt != null)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "This password reset link has already been used."
            };
        }

        if (resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = "This password reset link has expired."
            };
        }

        string passwordHash =
            BCrypt.Net.BCrypt.HashPassword(
                request.NewPassword,
                workFactor: 12);

        await _userRepository.UpdatePasswordAsync(
            resetToken.UserID,
            passwordHash);

        await _userRepository.MarkPasswordResetTokenUsedAsync(
            resetToken.ResetTokenID);
        await _activityLogService.LogAsync(
    resetToken.UserID,
    "User",
    null,
    "PASSWORD_RESET_COMPLETED",
    "AUTHENTICATION",
    "USER",
    resetToken.UserID,
    null,
    "Success",
    "Password was reset successfully.");
        return new ResetPasswordResponse
        {
            Success = true,
            Message = "Your password has been reset successfully."
        };
    }
}