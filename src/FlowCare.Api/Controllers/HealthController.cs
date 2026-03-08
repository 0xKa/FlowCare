using FlowCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowCare.Api.Controllers;

[ApiController]
[Route("api/health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Basic health check endpoint. Returns 200 OK if the API is running.
    /// </summary>
    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow });
    }

    [HttpPost("upload-test")]
    public async Task<IActionResult> UploadTest(IFormFile file, [FromServices] IFileStorageService fileStorage)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        await using var stream = file.OpenReadStream();
        if (!fileStorage.IsValidImage(stream))
            return BadRequest(new { error = "Invalid image file." });

        var tempPath = await fileStorage.SaveFileAsync(stream, "temp", $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
        return Ok(new { message = "File uploaded successfully.", path = tempPath });

    }

}
