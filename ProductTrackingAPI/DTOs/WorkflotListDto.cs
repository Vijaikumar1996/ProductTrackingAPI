namespace ProductTrackingAPI.DTOs;

public class WorkflowListDto
{
    public long Id { get; set; }

    public long DispatchId { get; set; }

    public string SaleOrderNo { get; set; } = string.Empty;

    public string? VehicleNumber { get; set; }

    public string? CustomerName { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactNumber { get; set; }

    public string Status { get; set; } = string.Empty;

    public int ExpectedCount { get; set; }

    public int ActualCount { get; set; }
}