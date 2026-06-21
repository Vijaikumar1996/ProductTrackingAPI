using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.DTOs;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;          

        public ReportController(
            IReportService reportService)
        {
            _reportService =
                reportService;
        }

        [HttpPost("mis-report")]
        public async Task<IActionResult> GetMisReport(
    MisReportRequestDto request)
        {
            return Ok(
                await _reportService.GetMisReportAsync(request));
        }
    }
}
