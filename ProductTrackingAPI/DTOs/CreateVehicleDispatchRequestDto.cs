namespace ProductTrackingAPI.DTOs;

public class CreateVehicleDispatchRequestDto
{
    public long SaleOrderId { get; set; }

    public string VehicleNumber { get; set; } = string.Empty;

    public string? DriverName { get; set; }

    public string? DriverMobile { get; set; }
}