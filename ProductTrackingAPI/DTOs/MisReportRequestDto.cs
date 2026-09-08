namespace ProductTrackingAPI.DTOs
{
    public class MisReportRequestDto
    {
        public string? SaleOrderNo { get; set; }

        public string? Status { get; set; }

        public bool? MismatchOnly { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
