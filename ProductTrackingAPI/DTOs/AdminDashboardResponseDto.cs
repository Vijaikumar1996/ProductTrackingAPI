namespace ProductTrackingAPI.DTOs
{
    public class AdminDashboardResponseDto
    {
        public int TodayUploads { get; set; }

        public int PendingHubReceive { get; set; }

        public int ProductsInHub { get; set; }

        public int OutForDelivery { get; set; }

        public int DeliveredToday { get; set; }

        public MissingPackageSummaryDto MissingPackages { get; set; } = new();

        public List<RecentOrderDto> RecentOrders { get; set; } = [];
    }

    public class MissingPackageSummaryDto
    {
        public int TotalMissingPackages { get; set; }

        public int HubReceiveMissing { get; set; }

        public int VehicleLoadingMissing { get; set; }

        public int DeliveryMissing { get; set; }
    }

    public class RecentOrderDto
    {
        public string SaleOrderNo { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public int ExpectedCount { get; set; }
        public int ScannedCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

   
}
