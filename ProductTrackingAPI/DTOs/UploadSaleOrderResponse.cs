namespace ProductTrackingAPI.DTOs
{
    public class UploadSaleOrderResponse
    {
        public bool Success { get; set; }

        public List<string> Errors { get; set; }
            = new();
    }
}
