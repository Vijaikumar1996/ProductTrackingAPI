namespace ProductTrackingAPI.DTOs;

public class VehicleDispatchListDto
{
    public long Id { get; set; }

    public string SaleOrderNo { get; set; } = string.Empty;

    public string VehicleNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}