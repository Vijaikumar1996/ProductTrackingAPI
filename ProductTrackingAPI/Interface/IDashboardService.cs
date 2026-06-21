using ProductTrackingAPI.DTOs;
using System.Security.Claims;

namespace ProductTrackingAPI.Interface
{
    public interface IDashboardService
    {
        Task<ScannerDashboardResponseDto>
        GetScannerDashboardAsync(
            ClaimsPrincipal user);

        Task<AdminDashboardResponseDto>
     GetAdminDashboardAsync();
    }
}
