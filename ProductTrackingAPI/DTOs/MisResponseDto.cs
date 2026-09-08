namespace ProductTrackingAPI.DTOs
{
    public class MisReportDto
    {
        public long SaleOrderId { get; set; }

        public string SaleOrderNo { get; set; } = string.Empty;

        public DateOnly ShipmentDate { get; set; }

        public int ExpectedHuCount { get; set; }

        public int ReceivedHuCount { get; set; }

        public int LoadedHuCount { get; set; }

        public int DeliveredHuCount { get; set; }

        public int HubMissingCount { get; set; }

        public int LoadingMissingCount { get; set; }

        public int DeliveryMissingCount { get; set; }

        public int TotalMissingCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
