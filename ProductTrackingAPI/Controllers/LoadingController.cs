using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/loading")]
public class LoadingController : ControllerBase
{
    private readonly ILoadingService _service;

    public LoadingController(
        ILoadingService service)
    {
        _service = service;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan(
        LoadingScanRequestDto request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        var result =
       await _service.ScanAsync(
           request,
           userId);

        return Ok(result);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(
     LoadingCompleteRequestDto request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        await _service.CompleteAsync(
            request,
            userId);

        return Ok(new
        {
            Message = "Loading completed"
        });
    }
}