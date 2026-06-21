namespace ProductTrackingAPI.DTOs;

public class HubSaleOrderDetailsDto
{
    public long SaleOrderId { get; set; }

    public string SaleOrderNo { get; set; } = string.Empty;

    public int ExpectedHuCount { get; set; }

    public int ReceivedHuCount { get; set; }

    public int PendingHuCount { get; set; }

    public string Status { get; set; } = string.Empty;
}