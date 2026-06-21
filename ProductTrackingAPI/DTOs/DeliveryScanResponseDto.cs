namespace ProductTrackingAPI.DTOs
{
    public class DeliveryScanResponseDto
    {
        public int ExpectedCount { get; set; }

        public int ActualCount { get; set; }

        public string Status { get; set; } =
            string.Empty;
    }
}
