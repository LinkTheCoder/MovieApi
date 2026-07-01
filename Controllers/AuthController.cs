using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger        = logger;
    }

    /// <summary>Loggar in och returnerar en JWT-token.</summary>
    /// <remarks>Användarnamn: admin, Lösenord: hemligt (hårdkodad för demo).</remarks>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        _logger.LogDebug("Login anropat – användare={Username}", model.Username);

        // För enkelhetens skull kör vi en hårdkodad kontroll (ersätt med databas)
        if (model.Username != "admin" || model.Password != "hemligt")
        {
            _logger.LogWarning("Misslyckat inloggningsförsök för användare={Username}.", model.Username);
            return Unauthorized("Ogiltiga användaruppgifter.");
        }

        // Skapa "claims" (information vi bakar in i tokenet)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, model.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Skapa själva tokenet
        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds
        );

        _logger.LogInformation("Användare {Username} loggade in framgångsrikt.", model.Username);
        return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}

// Enkel modell för inloggning
public class LoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

