using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface;

public interface IHubService
{
    Task<HubScanResponseDto> ScanAsync(
     HubScanRequestDto request,
     long userId);

    Task<HubSaleOrderDetailsDto> GetSaleOrderAsync(long saleOrderId);

    Task<List<HubSaleOrderListDto>> GetSaleOrdersAsync();

    Task CompleteReceivingAsync(long saleOrderId, long userId);
}