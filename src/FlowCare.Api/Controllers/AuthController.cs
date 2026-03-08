using FlowCare.Application.DTOs;
using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{

    /// <summary>
    /// Register a new customer. Requires a valid ID image upload.
    /// </summary>
    [HttpPost("register")]
    [Consumes("multipart/form-data")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Register(
        [FromForm] RegisterCustomerRequest request,
        [FromForm] IFormFile idImage)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { error = "Username, password, full name, and email are required." });

        if (idImage is null || idImage.Length == 0)
            return BadRequest(new { error = "ID image is required." });

        await using var imageStream = idImage.OpenReadStream();
        var (result, error, statusCode) = await authService.RegisterCustomerAsync(
            request,
            imageStream,
            idImage.FileName,
            idImage.Length);

        if (result is null)
            return StatusCode(statusCode, new { error });

        return Created("/api/auth/me", result);
    }

    /// <summary>
    /// Login using Basic Auth. Returns user info if credentials are valid.
    /// </summary>
    [HttpPost("login")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> Login()
    {
        // Auth handler already validated credentials. Extract user info from claims.
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var fullName = User.FindFirst("FullName")?.Value;

        if (userId is null || username is null || role is null || fullName is null)
            return Unauthorized(new { error = "Invalid claims." });

        var (result, error, statusCode) = await authService.LoginAsync(userId);
        if (result is null)
            return StatusCode(statusCode, new { error });

        return Ok(result);
    }
}
