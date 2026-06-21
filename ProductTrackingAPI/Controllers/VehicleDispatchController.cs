using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[ApiController]
[Route("api/dispatch")]
public class VehicleDispatchController : ControllerBase
{
    private readonly IVehicleDispatchService _service;

    public VehicleDispatchController(
        IVehicleDispatchService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateVehicleDispatchRequestDto request)
    {
        var userId = long.Parse(
            User.FindFirstValue(
                ClaimTypes.NameIdentifier)!);

        await _service.CreateAsync(
            request,
            userId);

        return Ok("Dispatch created successfully");
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result =
            await _service.GetDispatchesAsync();

        return Ok(result);
    }
}