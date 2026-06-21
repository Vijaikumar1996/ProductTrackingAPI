using ProductTrackingAPI.DTOs;

namespace ProductTrackingAPI.Interface;

public interface IVehicleDispatchService
{
    Task CreateAsync(
        CreateVehicleDispatchRequestDto request,
        long userId);

    Task<List<VehicleDispatchListDto>>
        GetDispatchesAsync();
}