using ProductTrackingAPI.DTOs;
using System.Security.Claims;

namespace ProductTrackingAPI.Interface
{
    public interface IScanHistoryService
    {
         Task<List<ScanHistoryDto>>
   GetScanHistoryAsync(
       ScanHistoryRequestDto request,
       ClaimsPrincipal user);
    }
}
