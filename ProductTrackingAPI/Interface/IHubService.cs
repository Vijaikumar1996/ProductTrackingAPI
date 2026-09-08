using ProductTrackingAPI.DTOs;
using static ProductTrackingAPI.DTOs.HubTruckDTO;

namespace ProductTrackingAPI.Interface;

public interface IHubService
{
    Task<HubScanResponseDto> ScanAsync(
     HubScanRequestDto request,
     long userId);

    Task<HubSaleOrderDetailsDto> GetSaleOrderAsync(long saleOrderId);

    Task<List<HubSaleOrderListDto>> GetSaleOrdersAsync();

    Task<List<HubTruckListDto>> GetHubTrucksAsync();

    Task<HubTruckScanResponse> ScanHubHuAsync(
    HubTruckScanRequest request, long userId);

    Task CompleteReceivingAsync(long saleOrderId, long userId);

    Task CompleteHubReceiveAsync(
    CompleteHubTruckRequest request,
    long userId);
}