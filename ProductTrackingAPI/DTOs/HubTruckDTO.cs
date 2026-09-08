namespace ProductTrackingAPI.DTOs
{
    public class HubTruckDTO
    {
        public class HubTruckListDto
        {
            public string VehicleNumber { get; set; } = string.Empty;

            public DateOnly ShipmentDate { get; set; }

            public string? DriverName { get; set; }

            public string? DriverMobile { get; set; }

            public int ExpectedHuCount { get; set; }

            public int ReceivedHuCount { get; set; }

            public int PendingHuCount { get; set; }

            public string Status { get; set; } = string.Empty;

            public List<HubTruckSaleOrderDto> SaleOrders { get; set; } = new();
        }


        public class HubTruckSaleOrderDto
        {
            public long SaleOrderId { get; set; }

            public string SaleOrderNo { get; set; } = string.Empty;

            public int ExpectedHuCount { get; set; }

            public int ReceivedHuCount { get; set; }

            public int PendingHuCount { get; set; }

            public string Status { get; set; } = string.Empty;
        }


        public class HubTruckScanRequest
        {
            public string VehicleNumber { get; set; } = string.Empty;

            public DateOnly ShipmentDate { get; set; }

            public string HuNumber { get; set; } = string.Empty;
        }


        public class HubTruckScanResponse
        {
            public string VehicleNumber { get; set; } = string.Empty;

            public DateOnly ShipmentDate { get; set; }

            public long SaleOrderId { get; set; }

            public string SaleOrderNo { get; set; } = string.Empty;

            public string HuNumber { get; set; } = string.Empty;

            public int TruckExpectedCount { get; set; }

            public int TruckReceivedCount { get; set; }

            public int TruckPendingCount { get; set; }

            public int SaleOrderExpectedCount { get; set; }

            public int SaleOrderReceivedCount { get; set; }

            public int SaleOrderPendingCount { get; set; }

            public string Message { get; set; } = string.Empty;
        }


        public class CompleteHubTruckRequest
        {
            public string VehicleNumber { get; set; } = string.Empty;

            public DateOnly ShipmentDate { get; set; }

            public bool ContinueWithMismatch { get; set; }
        }
    }
}
