using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HubController : ControllerBase
{
    private readonly IHubService _hubService;

    public HubController(IHubService hubService)
    {
        _hubService = hubService;
    }

    [HttpGet("saleorders")]
    public async Task<IActionResult> GetSaleOrders()
    {
        var result =
            await _hubService.GetSaleOrdersAsync();

        return Ok(result);
    }

    [HttpGet("saleorder/{saleOrderId}")]
    public async Task<IActionResult> GetSaleOrder(
    long saleOrderId)
    {
        var result =
            await _hubService.GetSaleOrderAsync(
                saleOrderId);

        return Ok(result);
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan(
        HubScanRequestDto request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        var result =
        await _hubService.ScanAsync(
            request,
            userId);

        return Ok(result);
    }

    [HttpPost("complete/{saleOrderId}")]
    public async Task<IActionResult> CompleteReceiving(
    long saleOrderId)
    {
        var userId = long.Parse(
           User.FindFirstValue(
               ClaimTypes.NameIdentifier)!);

        await _hubService.CompleteReceivingAsync(
            saleOrderId, userId);

        return Ok("Receiving completed");
    }
}