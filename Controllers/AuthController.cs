using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using ToDo.Models;
using ToDo.Services;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtHelper _jwtHelper;

    public AuthController(UserService userService, JwtHelper jwtHelper)
    {
        _userService = userService;
        _jwtHelper = jwtHelper;
    }
    
    //Register API
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        // Input Validation
        if (request == null ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Invalid registration details" });
        }

        try
        {
            // Set a default role if not provided
            string role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;

            
            _userService.CreateUser(request.Username, request.Password, role);

           
            var newUser = _userService.GetUserByUsername(request.Username);

            
            if (newUser == null)
            {
                return StatusCode(500, new { message = "User creation failed" });
            }

            // Generate initial access token
            var accessToken = _jwtHelper.GenerateAccessToken(newUser);

            // Generate refresh token
            var refreshToken = Guid.NewGuid().ToString();
            newUser.RefreshToken = refreshToken;
            _userService.UpdateUser(newUser);

            return CreatedAtAction(nameof(Register), new
            {
                username = newUser.Username,
                accessToken,
                refreshToken
            });
        }
        catch (InvalidOperationException ex)
        {
            // Handle duplicate username
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
          
            return StatusCode(500, new { message = "Registration failed", error = ex.Message });
        }
    }

    //Login API
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Input Validation
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Invalid login details" });
        }

        var user = _userService.GetUserByUsername(request.Username);

        
        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Generate JWT access and refresh tokens
        var accessToken = _jwtHelper.GenerateAccessToken(user);
        if (accessToken == null)
        {
            return StatusCode(500, new { message = "Token generation failed" });
        }

        var refreshToken = Guid.NewGuid().ToString();  // Generate a new refresh token

        // Store the refresh token securely 
        user.RefreshToken = refreshToken;
        _userService.UpdateUser(user);

        
        return Ok(new
        {
            accessToken,
            refreshToken
        });
    }
    
    //Refresh Token API
    [HttpPost("refresh-token")]
    [Authorize]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Input Validation
        if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Invalid refresh token request" });
        }

        var user = _userService.GetUserByUsername(request.Username);

        
        if (user == null || user.RefreshToken != request.RefreshToken)
        {
            return Unauthorized(new { message = "Invalid refresh token" });
        }

        // Generate a new JWT access token and refresh token
        var accessToken = _jwtHelper.GenerateAccessToken(user);
        if (accessToken == null)
        {
            return StatusCode(500, new { message = "Token generation failed" });
        }

        var newRefreshToken = Guid.NewGuid().ToString();

        
        user.RefreshToken = newRefreshToken;
        _userService.UpdateUser(user);

        
        return Ok(new
        {
            accessToken,
            refreshToken = newRefreshToken
        });
    }
    // Token Validation for Backend
    [HttpPost("validate-token")]
    [Authorize]
    public IActionResult ValidateToken([FromBody] TokenValidationRequest request)
    {
        // Input Validation
        if (request == null || string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Invalid token" });
        }

        var principal = _jwtHelper.GetPrincipalFromToken(request.Token);

        if (principal == null)
        {
            return Unauthorized(new { message = "Invalid or expired token" });
        }

        return Ok(new
        {
            message = "Token is valid",
            username = principal.Identity?.Name
        });
    }
}