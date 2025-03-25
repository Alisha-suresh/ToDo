using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using ToDo.Models;

public class JwtSettings
{
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public int AccessTokenExpirationHours { get; set; } = 1;
}

public class JwtHelper
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtHelper> _logger;

    public JwtHelper(IOptions<JwtSettings> jwtSettings, ILogger<JwtHelper> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public string? GenerateAccessToken(User user)
    {
        try
        {
            // Field Input Validation
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Role) ||
                string.IsNullOrEmpty(_jwtSettings.Key) || string.IsNullOrEmpty(_jwtSettings.Issuer) ||
                string.IsNullOrEmpty(_jwtSettings.Audience))
            {
                _logger.LogWarning("Unable to generate token due to missing required information");
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(_jwtSettings.AccessTokenExpirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating access token");
            return null;
        }
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string? token)
    {
        try
        {
            if (string.IsNullOrEmpty(token) ||
                string.IsNullOrEmpty(_jwtSettings.Key) ||
                string.IsNullOrEmpty(_jwtSettings.Issuer) ||
                string.IsNullOrEmpty(_jwtSettings.Audience))
            {
                _logger.LogWarning("Unable to validate token due to missing configuration");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return null;
        }
    }
}