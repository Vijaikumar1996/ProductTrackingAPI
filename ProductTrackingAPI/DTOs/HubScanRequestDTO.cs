namespace ProductTrackingAPI.DTOs;

public class HubScanRequestDto
{
    public long SaleOrderId { get; set; }

    public string HuNumber { get; set; } = string.Empty;
}