namespace ProductTrackingAPI.DTOs;

public class HubSaleOrderListDto
{
    public long Id { get; set; }

    public string SaleOrderNo { get; set; } = string.Empty;

    public int ExpectedHuCount { get; set; }

    public int ReceivedHuCount { get; set; }

    public string Status { get; set; } = string.Empty;
}