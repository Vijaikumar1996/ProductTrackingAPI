using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/delivery")]
public class DeliveryController : ControllerBase
{
    private readonly IDeliveryService _service;

    public DeliveryController(
        IDeliveryService service)
    {
        _service = service;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan(
        DeliveryScanRequestDto request)
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
    DeliveryCompleteRequestDto request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        await _service.CompleteAsync(
            request,
            userId);

        return Ok(new
        {
            Message = "Delivery completed"
        });
    }
}