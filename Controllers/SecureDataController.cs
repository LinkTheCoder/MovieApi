using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
[ApiController]
[Route("api/[controller]")]
[Authorize]
// Kräver att användaren har ett giltigt JWT 
public class SecureDataController : ControllerBase
{
    /// <summary>Hämtar skyddad data. Kräver ett giltigt JWT.</summary>
    [HttpGet]
    public IActionResult GetSecureData()
    {
        var userName =
        User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        return Ok(new
        {
            Message = $"Grattis {userName}, du har nått en skyddad endpoint!" }); 
        }
    } 