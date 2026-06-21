namespace ProductTrackingAPI.DTOs;

public class SaleOrderListDto
{
    public long Id { get; set; }

    public string SaleOrderNo { get; set; } = string.Empty;

    public DateTime ShipmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ExpectedHuCount { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }
}