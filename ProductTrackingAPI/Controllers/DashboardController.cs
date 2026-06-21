using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService
            _dashboardService;

        public DashboardController(
            IDashboardService dashboardService)
        {
            _dashboardService =
                dashboardService;
        }

        [HttpGet("scanner-dashboard")]
        public async Task<IActionResult>
            GetScannerDashboard()
        {
            var result =
                await _dashboardService
                    .GetScannerDashboardAsync(
                        User);

            return Ok(result);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult>
    GetDashboard()
        {
            var result =
                await _dashboardService
                    .GetAdminDashboardAsync();

            return Ok(result);
        }
    }
}