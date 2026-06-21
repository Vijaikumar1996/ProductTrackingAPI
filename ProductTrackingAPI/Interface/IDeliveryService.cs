using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface;

public interface IDeliveryService
{
    Task<DeliveryScanResponseDto> ScanAsync(
   DeliveryScanRequestDto request,
   long userId);

    public  Task CompleteAsync(
    DeliveryCompleteRequestDto request,
    long userId);
}