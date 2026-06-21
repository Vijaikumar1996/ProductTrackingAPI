using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface
{
    public interface IReportService
    {
        Task<PagedResponse<MisReportDto>>
    GetMisReportAsync(
        MisReportRequestDto request);
    }
}
