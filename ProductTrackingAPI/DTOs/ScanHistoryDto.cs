namespace ProductTrackingAPI.DTOs
{
    public class ScanHistoryDto
    {
        public long Id { get; set; }

        public string SaleOrderNo { get; set; } = string.Empty;

        public string HuNumber { get; set; } = string.Empty;

        public string ScanStage { get; set; } = string.Empty;

        public string ScannedBy { get; set; } = string.Empty;

        public DateTime ScanTime { get; set; }
    }
}
