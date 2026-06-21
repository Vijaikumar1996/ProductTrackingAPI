using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ScanHistoryController : ControllerBase
{
    private readonly IScanHistoryService
        _scanHistoryService;

    public ScanHistoryController(
        IScanHistoryService scanHistoryService)
    {
        _scanHistoryService =
            scanHistoryService;
    }

    [HttpPost("history")]
    public async Task<IActionResult>
        GetScanHistory(
            [FromBody]
            ScanHistoryRequestDto request)
    {
        var result =
            await _scanHistoryService
                .GetScanHistoryAsync(
                    request,
                    User);

        return Ok(result);
    }
}