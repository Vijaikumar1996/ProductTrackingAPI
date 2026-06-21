namespace ProductTrackingAPI.DTOs;

public class DeliveryScanRequestDto
{
    public long DispatchId { get; set; }

    public string HuNumber { get; set; } = string.Empty;
}