using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;
using ProductTrackingAPI.Services;
using System.Security.Claims;
using static ProductTrackingAPI.DTOs.HubTruckDTO;

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

    [HttpGet("trucks")]
    public async Task<IActionResult> GetHubTrucks()
    {
        var result =
            await _hubService.GetHubTrucksAsync();

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

    [HttpPost("truck/scan")]
    public async Task<IActionResult> ScanHubHu(
       [FromBody] HubTruckScanRequest request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        var result =
            await _hubService.ScanHubHuAsync(
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


    [HttpPost("complete")]
    public async Task<IActionResult> CompleteHubReceive(
        [FromBody] CompleteHubTruckRequest request)
    {

        var userId = long.Parse(
         User.FindFirstValue(
             ClaimTypes.NameIdentifier)!);

        await _hubService.CompleteHubReceiveAsync(
            request,
            userId);

        return Ok(new
        {
            message =
                "Hub receiving completed successfully."
        });
    }
}