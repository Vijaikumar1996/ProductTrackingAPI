namespace ProductTrackingAPI.DTOs;

public class SaleOrderDetailDto
{
    public long Id { get; set; }

    public string SaleOrderNo { get; set; } = string.Empty;

    public DateTime ShipmentDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ExpectedHuCount { get; set; }

    public int ReceivedHuCount { get; set; }

    public int LoadedHuCount { get; set; }

    public int DeliveredHuCount { get; set; }

    public string? CustomerName { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactNumber { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? VehicleNumber { get; set; }

    public string? InboundDriverName { get; set; }

    public string? InboundDriverMobile { get; set; }

    public string? CreatedByName { get; set; }

    public DateTime CreatedAt { get; set; }
}