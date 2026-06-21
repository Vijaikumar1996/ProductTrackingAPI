namespace ProductTrackingAPI.DTOs;

public class LoadingScanRequestDto
{
    public long SaleOrderId { get; set; }

    public string HuNumber { get; set; } = string.Empty;
}