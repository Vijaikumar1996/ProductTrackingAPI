namespace ProductTrackingAPI.DTOs
{
    public class LoadingCompleteRequestDto
    {
        public long SaleOrderId { get; set; }

        public string VehicleNumber { get; set; } = string.Empty;

        public string DriverName { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;
    }
}
