namespace ProductTrackingAPI.DTOs
{
    public class UploadSaleOrderRequest
    {
        public string SaleOrderNo { get; set; } = string.Empty;

        public DateTime ShipmentDate { get; set; }

        public string InboundVehicleNumber { get; set; } = string.Empty;

        public string? InboundDriverName { get; set; }

        public string? InboundDriverMobile { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string DeliveryAddress { get; set; } = string.Empty;

        public string? ContactPerson { get; set; }

        public string? ContactNumber { get; set; }

        public IFormFile File { get; set; } = default!;
    }
}
