using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface;

public interface ILoadingService
{
     Task<LoadingScanResponseDto> ScanAsync(
     LoadingScanRequestDto request,
     long userId);

    Task CompleteAsync(
    LoadingCompleteRequestDto request,
    long userId);
}