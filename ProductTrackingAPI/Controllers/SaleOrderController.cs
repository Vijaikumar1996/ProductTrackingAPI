
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;
using ProductTrackingAPI.Services;
using System.Security.Claims;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SaleOrderController : ControllerBase
{
    private readonly ISaleOrderService _service;

    public SaleOrderController(ISaleOrderService service)
    {
        _service = service;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(
    [FromForm] UploadSaleOrderRequest request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        await _service.UploadAsync(
            request,
            userId);

        return Ok(new
        {
            success = true,
            message = "Upload completed"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(
     [FromQuery] string? saleOrderNo,
     [FromQuery] DateTime? fromDate,
     [FromQuery] DateTime? toDate,
     [FromQuery] string? status)
    {
        var orders = await _service.GetOrdersAsync(
            saleOrderNo,
            fromDate,
            toDate,
            status);

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult>
GetOrderDetail(long id)
    {
        var order =
            await _service
                .GetOrderDetailAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}