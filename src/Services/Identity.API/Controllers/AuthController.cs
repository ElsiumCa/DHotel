using Identity.API.Services;
using Microsoft.AspNetCore.Mvc;
using Identity.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace DHotel.Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO request)
    {
        var result = await _authService.RegisterAsync(request);
        if(!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO request)
    {
        var result = await _authService.LoginAsync(request);
        if(!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request)
    {
        var result = await _authService.RefreshTokenAsync(request);
        if(!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if(userId == null)
        {
            return Unauthorized();
        }
        await _authService.LogoutAsync(userId);
        return Ok(new {message = "Logout successful"});
    }
}