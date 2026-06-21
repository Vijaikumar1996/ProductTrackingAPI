namespace ProductTrackingAPI.DTOs
{
    public class ScannerDashboardResponseDto
    {
        public List<DashboardCardDto> Cards { get; set; }
            = new();

        public int ScannedHuCount { get; set; }

        public List<RecentScanDto> RecentScans { get; set; }
            = new();
    }

    public class DashboardCardDto
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public int Count { get; set; }
    }

    public class RecentScanDto
    {
        public string SaleOrderNo { get; set; }
            = string.Empty;

        public string HuNumber { get; set; }
            = string.Empty;

        public string ScanStage { get; set; }
            = string.Empty;

        public DateTime ScanTime { get; set; }
    }
}
