using Brokerage.Business.Services;
using Brokerage.Models.DTOs.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Brokerage.API.Controllers;


// Handles user authentication, registration, identity verification, and password recovery.

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Registers a new user account in the system based on their assigned role.
    /// </summary>
    /// <remarks>
    /// Flow: Validates input role -> Checks email uniqueness -> Hashes password -> Creates user entity -> Logs activity.
    /// </remarks>
    /// <param name="request">User registration details including role and credentials.</param>
    /// <returns>Status response indicating registration success or failure.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        RegisterResponse response =
            await _authService.RegisterAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Authenticates a user and generates a JWT bearer token.
    /// </summary>
    /// <remarks>
    /// Flow: Verifies email &amp; active status -> Validates password hash -> Generates JWT token -> Logs activity.
    /// </remarks>
    /// <param name="request">Login request containing email and password.</param>
    /// <returns>JWT authentication token and user profile details.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        LoginResponse response = await _authService.LoginAsync(request);
        if (!response.Success)
        {
            return Unauthorized(response);
        }
        return Ok(response);
    }

    /// <summary>
    /// Validates the current authenticated user session and returns identity claims.
    /// </summary>
    /// <remarks>
    /// Flow: Reads claims from current JWT principal -> Returns user identity data.
    /// </remarks>
    /// <returns>User identity claim details.</returns>
    [Authorize]
    [HttpPost("check")]
    public async Task<IActionResult> Check()
    {
        string? userId =
       User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        string? name =
            User.FindFirst(ClaimTypes.Name)?.Value;

        string? email =
            User.FindFirst(ClaimTypes.Email)?.Value;

        string? role =
            User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            UserId = userId,
            Name = name,
            Email = email,
            Role = role
        });
    }

    /// <summary>
    /// Generates a password reset token and emails a reset link to the user.
    /// </summary>
    /// <remarks>
    /// Flow: Checks user existence -> Invalidates old tokens -> Generates SHA256 hashed token -> Emails reset link.
    /// </remarks>
    /// <param name="request">Password reset request containing user email.</param>
    /// <returns>Acknowledgment response for password reset attempt.</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    ForgotPasswordRequest request)
    {
        ForgotPasswordResponse response =
            await _authService.ForgotPasswordAsync(request);

        return Ok(response);
    }

    /// <summary>
    /// Resets user password using a valid reset token.
    /// </summary>
    /// <remarks>
    /// Flow: Validates token validity/expiration -> Hashes new password -> Updates database credentials -> Marks token used.
    /// </remarks>
    /// <param name="request">Reset payload containing reset token and new password.</param>
    /// <returns>Operation result status.</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
    ResetPasswordRequest request)
    {
        ResetPasswordResponse response =
            await _authService.ResetPasswordAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}