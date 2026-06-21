
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        var result =
            await _authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(
                "Invalid credentials");

        return Ok(result);
    }

    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(
    [FromBody] ChangePasswordRequesDto request)
    {
        var result =
            await _authService
                .ChangePasswordAsync(
                    request,
                    User);

        return Ok(new
        {
            success = true,
            message = result
        });
    }
}