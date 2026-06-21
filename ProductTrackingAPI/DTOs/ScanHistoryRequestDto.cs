namespace ProductTrackingAPI.DTOs
{
    public class ScanHistoryRequestDto
    {
        public string? HuNumber { get; set; }

        public string? SaleOrderNo { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        // Admin only
        public string? ScanStage { get; set; }

        public long? UserId { get; set; }
    }
}
